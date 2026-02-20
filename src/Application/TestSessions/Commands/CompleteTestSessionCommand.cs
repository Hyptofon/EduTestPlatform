using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Tests.Dtos;
using Application.TestSessions.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.TestSessions.Commands;

public record CompleteTestSessionCommand(Guid SessionId)
    : IRequest<Either<TestSessionException, TestSession>>;

public class CompleteTestSessionCommandHandler(
    ITestSessionRepository sessionRepository,
    ITestRepository testRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<CompleteTestSessionCommand, Either<TestSessionException, TestSession>>
{
    public async Task<Either<TestSessionException, TestSession>> Handle(
        CompleteTestSessionCommand request,
        CancellationToken cancellationToken)
    {
        var sessionId = new TestSessionId(request.SessionId);
        var sessionOption = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        return await sessionOption.MatchAsync(
            session => CompleteSession(session, cancellationToken),
            () => Task.FromResult<Either<TestSessionException, TestSession>>(
                new TestSessionNotFoundException(sessionId)));
    }

    private async Task<Either<TestSessionException, TestSession>> CompleteSession(
        TestSession session,
        CancellationToken cancellationToken)
    {
        if (session.Status != TestSessionStatus.InProgress)
        {
            return new TestSessionNotInProgressException(session.Id);
        }

        try
        {
            var testOption = await testRepository.GetByIdAsync(session.TestId, cancellationToken);
            if (testOption.IsNone)
            {
                return new UnhandledTestSessionException(session.Id, null);
            }

            var test = testOption.IfNone(() => throw new InvalidOperationException());
            
            var contentJson = System.Text.Json.JsonSerializer.Deserialize<TestContentForGradingDto>(test.ContentJson);
            
            if (contentJson?.Questions != null)
            {
                foreach (var answer in session.Answers)
                {
                    var question = contentJson.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question == null) continue;

                    var points = GradeAnswer(answer, question);
                    answer.Grade(points);
                }
            }

            session.Complete();

            sessionRepository.Update(session);
            await dbContext.SaveChangesAsync(cancellationToken);

            return session;
        }
        catch (Exception exception)
        {
            return new UnhandledTestSessionException(session.Id, exception);
        }
    }

    private int GradeAnswer(StudentAnswer studentAnswer, QuestionForGradingDto question)
    {
        return question.Type switch
        {
            "SingleChoice" => GradeSingleChoice(studentAnswer, question),
            "MultipleChoice" => GradeMultipleChoice(studentAnswer, question),
            "ShortAnswer" => GradeShortAnswer(studentAnswer, question),
            "OpenEssay" => 0, // руцями переверяє викладач аля есе і т.д.
            _ => 0
        };
    }

    private int GradeSingleChoice(StudentAnswer studentAnswer, QuestionForGradingDto question)
    {
        if (studentAnswer.SelectedAnswerIds == null || !studentAnswer.SelectedAnswerIds.Any())
            return 0;

        var selectedId = studentAnswer.SelectedAnswerIds.First();
        var correctAnswer = question.Answers?.FirstOrDefault(a => a.IsCorrect);

        return correctAnswer?.Id == selectedId ? question.Points : 0;
    }

    private int GradeMultipleChoice(StudentAnswer studentAnswer, QuestionForGradingDto question)
    {
        if (studentAnswer.SelectedAnswerIds == null || !studentAnswer.SelectedAnswerIds.Any())
            return 0;

        var correctIds = question.Answers?
            .Where(a => a.IsCorrect)
            .Select(a => a.Id)
            .ToHashSet() ?? new System.Collections.Generic.HashSet<Guid>();

        var selectedIds = studentAnswer.SelectedAnswerIds.ToHashSet();

        // Повні бали при точному співпадінні
        if (correctIds.SetEquals(selectedIds))
            return question.Points;

        // Часткові бали: (правильні - помилкові) / всього правильних * бали
        var correctSelected = selectedIds.Intersect(correctIds).Count();
        var incorrectSelected = selectedIds.Except(correctIds).Count();

        if (correctIds.Count == 0)
            return 0;

        var partialScore = (correctSelected - incorrectSelected) / (double)correctIds.Count * question.Points;

        return Math.Max(0, (int)Math.Round(partialScore));
    }
    
    private int GradeShortAnswer(StudentAnswer studentAnswer, QuestionForGradingDto question)
    {
        if (string.IsNullOrWhiteSpace(studentAnswer.TextAnswer))
            return 0;
        
        var correctAnswers = question.Answers?.Where(a => a.IsCorrect).ToList();
        
        if (correctAnswers == null || !correctAnswers.Any())
            return 0;

        // Нормалізація тексту студента: trim, видалення зайвих пробілів, lowercase
        var studentText = NormalizeText(studentAnswer.TextAnswer);
        
        // Перевіряємо чи співпадає з будь-якою правильною відповіддю
        var isCorrect = correctAnswers.Any(a => NormalizeText(a.Text) == studentText);

        return isCorrect ? question.Points : 0;
    }

    /// <summary>
    /// Нормалізує текст: trim, видаляє зайві пробіли, приводить до нижнього регістру.
    /// </summary>
    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Видаляємо пробіли на початку і в кінці
        var trimmed = text.Trim();

        // Замінюємо множинні пробіли на один
        var normalized = System.Text.RegularExpressions.Regex.Replace(trimmed, @"\s+", " ");

        return normalized.ToLowerInvariant();
    }
}
