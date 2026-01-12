using Domain.Organizations;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IOrganizationRepository
{
    void Add(Organization entity);
    void Update(Organization entity);
    void Delete(Organization entity);
    Task<Option<Organization>> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken);
    Task<Option<Organization>> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken cancellationToken);
}