using Domain.Invites;
using Domain.Organizations;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IInviteCodeRepository
{
    void Add(InviteCode entity);
    void Update(InviteCode entity);
    void Delete(InviteCode entity);
    Task<Option<InviteCode>> GetByIdAsync(InviteCodeId id, CancellationToken cancellationToken);
    Task<Option<InviteCode>> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<InviteCode>> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InviteCode>> GetActiveByOrganizationIdAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}