using Domain.Organizations;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IOrganizationalUnitRepository
{
    void Add(OrganizationalUnit entity);
    void Update(OrganizationalUnit entity);
    void Delete(OrganizationalUnit entity);
    Task<Option<OrganizationalUnit>> GetByIdAsync(OrganizationalUnitId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrganizationalUnit>> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrganizationalUnit>> GetByParentIdAsync(OrganizationalUnitId parentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrganizationalUnit>> GetRootUnitsAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}