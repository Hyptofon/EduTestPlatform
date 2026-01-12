using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Tests;
using MediatR;

namespace Application.TestSessions.Queries;

public record GetMyTestSessionsQuery : IRequest<IReadOnlyList<TestSession>>;

public class GetMyTestSessionsQueryHandler(
    ITestSessionRepository sessionRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetMyTestSessionsQuery, IReadOnlyList<TestSession>>
{
    public async Task<IReadOnlyList<TestSession>> Handle(
        GetMyTestSessionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            return Array.Empty<TestSession>();
        }

        return await sessionRepository.GetByStudentIdAsync(
            currentUserService.UserId.Value,
            cancellationToken);
    }
}