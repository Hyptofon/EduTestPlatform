using Application.Common.Interfaces;
using Domain.Enrollments;
using Domain.Organizations;
using Domain.TestSessions;
using Domain.Tests;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TestSessions.Commands.StartTestSession;

public record StartTestSessionCommand : IRequest<Either<Exception, TestSession>>
{
    public required Guid TestId { get; init; }
}

public class StartTestSessionCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<StartTestSessionCommand, Either<Exception, TestSession>>
{
    public async Task<Either<Exception, TestSession>> Handle(
        StartTestSessionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                return new UnauthorizedAccessException("User must be authenticated");
            }

            var userId = currentUserService.UserId.Value;
            var testId = new TestId(request.TestId);

            // 1. Перевірка існування тесту
            var test = await context.Tests
                .FirstOrDefaultAsync(x => x.Id == testId, cancellationToken);

            if (test == null)
            {
                return new KeyNotFoundException($"Test with ID {request.TestId} not found");
            }

            // 2. Перевірка, що тест не є чернеткою
            if (test.IsDraft)
            {
                return new InvalidOperationException("Cannot start a draft test");
            }

            // 3. Перевірка часового вікна
            var now = DateTime.UtcNow;
            if (test.Settings.StartDate.HasValue && now < test.Settings.StartDate.Value)
            {
                return new InvalidOperationException($"Test will be available from {test.Settings.StartDate.Value:g}");
            }

            if (test.Settings.EndDate.HasValue && now > test.Settings.EndDate.Value)
            {
                return new InvalidOperationException("Test time window has expired");
            }

            // 4. Перевірка, що студент записаний на предмет
            var isEnrolled = await context.StudentSubjects
                .AnyAsync(x => x.StudentId == userId && 
                              x.SubjectId == test.SubjectId && 
                              x.IsActive, 
                          cancellationToken);

            if (!isEnrolled)
            {
                return new InvalidOperationException("You must be enrolled in the subject to take this test");
            }

            // 5. Перевірка кількості спроб
            var attemptCount = await context.TestSessions
                .CountAsync(x => x.TestId == testId && x.StudentId == userId, cancellationToken);

            if (attemptCount >= test.Settings.MaxAttempts)
            {
                return new InvalidOperationException($"Maximum number of attempts ({test.Settings.MaxAttempts}) reached");
            }

            // 6. Перевірка на активну сесію
            var activeSession = await context.TestSessions
                .FirstOrDefaultAsync(
                    x => x.TestId == testId && 
                         x.StudentId == userId && 
                         x.Status == TestSessionStatus.InProgress,
                    cancellationToken);

            if (activeSession != null)
            {
                return new InvalidOperationException("You already have an active session for this test");
            }

            // 7. Створення нової сесії
            var session = TestSession.Start(testId, userId, now);

            context.TestSessions.Add(session);
            await context.SaveChangesAsync(cancellationToken);

            return session;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}