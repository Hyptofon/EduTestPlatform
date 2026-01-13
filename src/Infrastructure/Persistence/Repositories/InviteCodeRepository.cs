using Application.Common.Interfaces.Repositories;
using Domain.Invites;
using Domain.Organizations;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class InviteCodeRepository(ApplicationDbContext context) : IInviteCodeRepository
{
    public void Add(InviteCode entity)
    {
        context.InviteCodes.Add(entity);
    }

    public void Update(InviteCode entity)
    {
        context.InviteCodes.Update(entity);
    }

    public void Delete(InviteCode entity)
    {
        context.InviteCodes.Remove(entity);
    }

    public async Task<Option<InviteCode>> GetByIdAsync(InviteCodeId id, CancellationToken cancellationToken)
    {
        var entity = await context.InviteCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<InviteCode>.None;
    }

    public async Task<Option<InviteCode>> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var entity = await context.InviteCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

        return entity ?? Option<InviteCode>.None;
    }

    public async Task<IReadOnlyList<InviteCode>> GetByOrganizationIdAsync(
        OrganizationId organizationId, 
        CancellationToken cancellationToken)
    {
        return await context.InviteCodes
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InviteCode>> GetActiveByOrganizationIdAsync(
        OrganizationId organizationId, 
        CancellationToken cancellationToken)
    {
        return await context.InviteCodes
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}