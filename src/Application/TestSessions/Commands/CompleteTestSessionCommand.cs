using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
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
            "OpenEssay" => 0,
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

        return correctIds.SetEquals(selectedIds) ? question.Points : 0;
    }

    private int GradeShortAnswer(StudentAnswer studentAnswer, QuestionForGradingDto question)
    {
        if (string.IsNullOrWhiteSpace(studentAnswer.TextAnswer))
            return 0;

        var correctAnswer = question.Answers?.FirstOrDefault(a => a.IsCorrect);
        if (correctAnswer == null)
            return 0;

        var studentText = studentAnswer.TextAnswer.Trim().ToLower();
        var correctText = correctAnswer.Text.Trim().ToLower();

        return studentText == correctText ? question.Points : 0;
    }
}

public record TestContentForGradingDto
{
    public List<QuestionForGradingDto>? Questions { get; init; }
}

public record QuestionForGradingDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public int Points { get; init; }
    public List<AnswerForGradingDto>? Answers { get; init; }
}

public record AnswerForGradingDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}