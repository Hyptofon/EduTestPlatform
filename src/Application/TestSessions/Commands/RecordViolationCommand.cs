using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.TestSessions.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.TestSessions.Commands;

public record RecordViolationCommand : IRequest<Either<TestSessionException, TestSession>>
{
    public required Guid SessionId { get; init; }
    public required string ViolationType { get; init; }
    public Guid? QuestionId { get; init; }
    public int DurationSeconds { get; init; }
}

public class RecordViolationCommandHandler(
    ITestSessionRepository sessionRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<RecordViolationCommand, Either<TestSessionException, TestSession>>
{
    public async Task<Either<TestSessionException, TestSession>> Handle(
        RecordViolationCommand request,
        CancellationToken cancellationToken)
    {
        var sessionId = new TestSessionId(request.SessionId);
        var sessionOption = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        return await sessionOption.MatchAsync(
            session => RecordViolation(session, request, cancellationToken),
            () => Task.FromResult<Either<TestSessionException, TestSession>>(
                new TestSessionNotFoundException(sessionId)));
    }

    private async Task<Either<TestSessionException, TestSession>> RecordViolation(
        TestSession session,
        RecordViolationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var violation = request.ViolationType.ToLower() switch
            {
                "focuslost" => Violation.FocusLost(request.QuestionId, request.DurationSeconds),
                "tabswitch" => Violation.TabSwitch(request.QuestionId),
                _ => Violation.TabSwitch(request.QuestionId)
            };

            session.RecordViolation(violation);

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