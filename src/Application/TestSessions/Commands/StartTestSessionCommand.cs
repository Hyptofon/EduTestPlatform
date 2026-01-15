using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Tests.Exceptions;
using Application.TestSessions.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;
using System.Text.Json; 
namespace Application.TestSessions.Commands;

public record StartTestSessionCommand : IRequest<Either<TestSessionException, TestSession>>
{
    public required Guid TestId { get; init; }
    public required Guid StudentId { get; init; }
}

public class StartTestSessionCommandHandler(
    ITestSessionRepository sessionRepository,
    ITestRepository testRepository,
    ISubjectEnrollmentRepository enrollmentRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<StartTestSessionCommand, Either<TestSessionException, TestSession>>
{
    public async Task<Either<TestSessionException, TestSession>> Handle(
        StartTestSessionCommand request,
        CancellationToken cancellationToken)
    {
        var testId = new TestId(request.TestId);
        var testOption = await testRepository.GetByIdAsync(testId, cancellationToken);

        if (testOption.IsNone)
        {
            return new TestSessionNotFoundException(TestSessionId.Empty());
        }

        var test = testOption.IfNone(() => throw new InvalidOperationException());
        
        var now = DateTime.UtcNow;

        if (test.Settings.StartDate.HasValue && now < test.Settings.StartDate.Value)
        {
            return new TestAvailabilityException(testId, "Test has not started yet.");
        }

        if (test.Settings.EndDate.HasValue && now > test.Settings.EndDate.Value)
        {
            return new TestAvailabilityException(testId, "Test execution period has ended.");
        }
        
        if (!test.IsAccessible())
        {
            return new TestAvailabilityException(testId, "Test is currently not accessible.");
        }

        var isEnrolled = await enrollmentRepository.IsUserEnrolledAsync(
            test.SubjectId,
            request.StudentId,
            cancellationToken);

        if (!isEnrolled)
        {
            return new UnauthorizedSessionAccessException(TestSessionId.Empty());
        }

        var activeSession = await sessionRepository.GetActiveSessionAsync(testId, request.StudentId, cancellationToken);
        if (activeSession.IsSome)
        {
            return new TestSessionAlreadyExistsException(testId, request.StudentId);
        }

        var attemptCount = await sessionRepository.GetAttemptCountAsync(testId, request.StudentId, cancellationToken);
        if (attemptCount >= test.Settings.MaxAttempts)
        {
            return new MaxAttemptsReachedException(testId, request.StudentId, test.Settings.MaxAttempts);
        }

        return await CreateSession(testId, request.StudentId, test, cancellationToken);
    }

    private async Task<Either<TestSessionException, TestSession>> CreateSession(
        TestId testId,
        Guid studentId,
        Test test,
        CancellationToken cancellationToken)
    {
        try
        {
            var contentJson = JsonSerializer.Deserialize<TestContentDto>(test.ContentJson);
            var questions = contentJson?.Questions ?? new List<QuestionDto>();
            
            int maxScore;

            if (test.Settings.BankModeQuestionCount.HasValue && 
                test.Settings.BankModeQuestionCount.Value > 0 &&
                test.Settings.BankModeQuestionCount.Value < questions.Count)
            {
                maxScore = questions
                    .Take(test.Settings.BankModeQuestionCount.Value)
                    .Sum(q => q.Points);
            }
            else
            {
                maxScore = questions.Sum(q => q.Points);
            }

            var session = TestSession.New(testId, studentId, maxScore);

            sessionRepository.Add(session);
            await dbContext.SaveChangesAsync(cancellationToken);

            return session;
        }
        catch (Exception exception)
        {
            return new UnhandledTestSessionException(TestSessionId.Empty(), exception);
        }
    }
}

public record TestContentDto
{
    public List<QuestionDto>? Questions { get; init; }
}

public record QuestionDto
{
    public int Points { get; init; }
}