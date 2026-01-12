using Application.Common.Interfaces.Repositories;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.TestSessions.Queries;

public record GetTestSessionByIdQuery(Guid SessionId) : IRequest<Option<TestSession>>;

public class GetTestSessionByIdQueryHandler(ITestSessionRepository sessionRepository)
    : IRequestHandler<GetTestSessionByIdQuery, Option<TestSession>>
{
    public async Task<Option<TestSession>> Handle(
        GetTestSessionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var sessionId = new TestSessionId(request.SessionId);
        return await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
    }
}