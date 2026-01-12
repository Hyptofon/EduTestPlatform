using Application.Common.Interfaces.Repositories;
using Domain.Invites;
using Domain.Organizations;
using MediatR;

namespace Application.Invites.Queries;

public record GetInviteCodesByOrganizationQuery(Guid OrganizationId, bool OnlyActive = false) 
    : IRequest<IReadOnlyList<InviteCode>>;

public class GetInviteCodesByOrganizationQueryHandler(IInviteCodeRepository inviteCodeRepository)
    : IRequestHandler<GetInviteCodesByOrganizationQuery, IReadOnlyList<InviteCode>>
{
    public async Task<IReadOnlyList<InviteCode>> Handle(
        GetInviteCodesByOrganizationQuery request,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(request.OrganizationId);

        return request.OnlyActive
            ? await inviteCodeRepository.GetActiveByOrganizationIdAsync(organizationId, cancellationToken)
            : await inviteCodeRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
    }
}