using Application.Common.Interfaces.Repositories;
using Domain.Tests;
using MediatR;

namespace Application.TestSessions.Queries;

public record GetSessionsByTestQuery(Guid TestId) : IRequest<IReadOnlyList<TestSession>>;

public class GetSessionsByTestQueryHandler(ITestSessionRepository sessionRepository)
    : IRequestHandler<GetSessionsByTestQuery, IReadOnlyList<TestSession>>
{
    public async Task<IReadOnlyList<TestSession>> Handle(
        GetSessionsByTestQuery request,
        CancellationToken cancellationToken)
    {
        var testId = new TestId(request.TestId);
        return await sessionRepository.GetByTestIdAsync(testId, cancellationToken);
    }
}