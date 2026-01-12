using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.TestSessions.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.TestSessions.Commands;

public record GradeAnswerCommand : IRequest<Either<TestSessionException, TestSession>>
{
    public required Guid SessionId { get; init; }
    public required Guid QuestionId { get; init; }
    public required int Points { get; init; }
}

public class GradeAnswerCommandHandler(
    ITestSessionRepository sessionRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<GradeAnswerCommand, Either<TestSessionException, TestSession>>
{
    public async Task<Either<TestSessionException, TestSession>> Handle(
        GradeAnswerCommand request,
        CancellationToken cancellationToken)
    {
        var sessionId = new TestSessionId(request.SessionId);
        var sessionOption = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        return await sessionOption.MatchAsync(
            session => GradeAnswer(session, request, cancellationToken),
            () => Task.FromResult<Either<TestSessionException, TestSession>>(
                new TestSessionNotFoundException(sessionId)));
    }

    private async Task<Either<TestSessionException, TestSession>> GradeAnswer(
        TestSession session,
        GradeAnswerCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            session.GradeAnswer(request.QuestionId, request.Points);
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