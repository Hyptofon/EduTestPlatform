using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.OrganizationalUnits.Queries;

public record GetOrganizationalUnitByIdQuery(Guid UnitId) : IRequest<Option<OrganizationalUnit>>;

public class GetOrganizationalUnitByIdQueryHandler(IOrganizationalUnitRepository unitRepository)
    : IRequestHandler<GetOrganizationalUnitByIdQuery, Option<OrganizationalUnit>>
{
    public async Task<Option<OrganizationalUnit>> Handle(
        GetOrganizationalUnitByIdQuery request,
        CancellationToken cancellationToken)
    {
        var unitId = new OrganizationalUnitId(request.UnitId);
        return await unitRepository.GetByIdAsync(unitId, cancellationToken);
    }
}