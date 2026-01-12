using Domain.Organizations;
using Domain.Users;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IUserOrganizationRepository
{
    void Add(UserOrganization entity);
    void Update(UserOrganization entity);
    void Delete(UserOrganization entity);
    Task<Option<UserOrganization>> GetByIdAsync(UserOrganizationId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserOrganization>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserOrganization>> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken cancellationToken);
    Task<Option<UserOrganization>> GetByUserAndOrganizationAsync(Guid userId, OrganizationId organizationId, CancellationToken cancellationToken);
}