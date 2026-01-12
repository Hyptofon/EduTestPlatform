using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using MediatR;

namespace Application.OrganizationalUnits.Queries;

public record GetUnitsByOrganizationQuery(Guid OrganizationId) : IRequest<IReadOnlyList<OrganizationalUnit>>;

public class GetUnitsByOrganizationQueryHandler(IOrganizationalUnitRepository unitRepository)
    : IRequestHandler<GetUnitsByOrganizationQuery, IReadOnlyList<OrganizationalUnit>>
{
    public async Task<IReadOnlyList<OrganizationalUnit>> Handle(
        GetUnitsByOrganizationQuery request,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(request.OrganizationId);
        return await unitRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
    }
}