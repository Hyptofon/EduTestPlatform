using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.TestSessions.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.TestSessions.Commands;

public record SubmitAnswerCommand : IRequest<Either<TestSessionException, TestSession>>
{
    public required Guid SessionId { get; init; }
    public required Guid QuestionId { get; init; }
    public List<Guid>? SelectedAnswerIds { get; init; }
    public string? TextAnswer { get; init; }
}

public class SubmitAnswerCommandHandler(
    ITestSessionRepository sessionRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<SubmitAnswerCommand, Either<TestSessionException, TestSession>>
{
    public async Task<Either<TestSessionException, TestSession>> Handle(
        SubmitAnswerCommand request,
        CancellationToken cancellationToken)
    {
        var sessionId = new TestSessionId(request.SessionId);
        var sessionOption = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        return await sessionOption.MatchAsync(
            session => SubmitAnswer(session, request, cancellationToken),
            () => Task.FromResult<Either<TestSessionException, TestSession>>(
                new TestSessionNotFoundException(sessionId)));
    }

    private async Task<Either<TestSessionException, TestSession>> SubmitAnswer(
        TestSession session,
        SubmitAnswerCommand request,
        CancellationToken cancellationToken)
    {
        if (session.Status != TestSessionStatus.InProgress)
        {
            return new TestSessionNotInProgressException(session.Id);
        }

        try
        {
            var answer = StudentAnswer.New(
                request.QuestionId,
                request.SelectedAnswerIds,
                request.TextAnswer);

            session.SubmitAnswer(answer);

            sessionRepository.Update(session);
            await dbContext.SaveChangesAsync(cancellationToken);

            return session;
        }
        catch (Exception exception)
        {
            return new UnhandledTestSessionException(session.Id, exception);
        }
    }
}