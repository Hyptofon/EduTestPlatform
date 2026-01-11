using Application.Common.Interfaces;
using Domain.TestSessions;
using Domain.Tests;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TestSessions.Commands.CompleteTestSession;

public record CompleteTestSessionCommand : IRequest<Either<Exception, TestSession>>
{
    public required Guid SessionId { get; init; }
}

public class CompleteTestSessionCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<CompleteTestSessionCommand, Either<Exception, TestSession>>
{
    public async Task<Either<Exception, TestSession>> Handle(
        CompleteTestSessionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                return new UnauthorizedAccessException("User must be authenticated");
            }

            var userId = currentUserService.UserId.Value;
            var sessionId = new TestSessionId(request.SessionId);

            var session = await context.TestSessions
                .Include(x => x.Answers)
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

            if (session == null)
            {
                return new KeyNotFoundException("Session not found");
            }

            if (session.StudentId != userId)
            {
                return new UnauthorizedAccessException("Unauthorized");
            }

            if (session.Status != TestSessionStatus.InProgress)
            {
                return new InvalidOperationException("Session is not active");
            }

            // Завершуємо сесію
            session.Complete(DateTime.UtcNow);

            // Рахуємо бал (автоматична перевірка)
            var test = await context.Tests
                .FirstOrDefaultAsync(x => x.Id == session.TestId, cancellationToken);

            if (test != null)
            {
                int totalScore = 0;

                foreach (var answer in session.Answers)
                {
                    // Знаходимо питання в тесті
                    Question? question = null;
                    foreach (var section in test.Content.Sections)
                    {
                        question = section.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                        if (question != null) break;
                    }

                    if (question == null) continue;

                    // Перевірка відповіді
                    switch (question.Type)
                    {
                        case QuestionType.SingleChoice:
                        case QuestionType.MultipleChoice:
                            var correctOptions = question.Options
                                .Where(o => o.IsCorrect)
                                .Select(o => o.Id)
                                .OrderBy(id => id)
                                .ToList();

                            var selectedOptions = answer.SelectedOptionIds
                                .OrderBy(id => id)
                                .ToList();

                            if (correctOptions.SequenceEqual(selectedOptions))
                            {
                                totalScore += question.Points;
                                answer.PointsAwarded = question.Points;
                            }
                            break;

                        case QuestionType.ShortAnswer:
                            // Тут потрібна точна перевірка тексту (можна додати нормалізацію)
                            var correctAnswer = question.Options.FirstOrDefault(o => o.IsCorrect)?.Text;
                            if (!string.IsNullOrEmpty(correctAnswer) && 
                                string.Equals(answer.TextAnswer?.Trim(), correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                totalScore += question.Points;
                                answer.PointsAwarded = question.Points;
                            }
                            break;

                        case QuestionType.OpenEssay:
                            // Потребує ручної перевірки
                            answer.IsMarkedManually = true;
                            break;
                    }
                }

                session.SetScore(totalScore);
            }

            context.TestSessions.Update(session);
            await context.SaveChangesAsync(cancellationToken);

            return session;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}