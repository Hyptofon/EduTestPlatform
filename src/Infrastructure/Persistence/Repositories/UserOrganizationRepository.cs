using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using Domain.Users;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserOrganizationRepository(ApplicationDbContext context) : IUserOrganizationRepository
{
    public void Add(UserOrganization entity)
    {
        context.UserOrganizations.Add(entity);
    }

    public void Update(UserOrganization entity)
    {
        context.UserOrganizations.Update(entity);
    }

    public void Delete(UserOrganization entity)
    {
        context.UserOrganizations.Remove(entity);
    }

    public async Task<Option<UserOrganization>> GetByIdAsync(
        UserOrganizationId id, 
        CancellationToken cancellationToken)
    {
        var entity = await context.UserOrganizations
            .AsNoTracking()
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<UserOrganization>.None;
    }

    public async Task<IReadOnlyList<UserOrganization>> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return await context.UserOrganizations
            .AsNoTracking()
            .Include(x => x.Organization)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserOrganization>> GetByOrganizationIdAsync(
        OrganizationId organizationId, 
        CancellationToken cancellationToken)
    {
        return await context.UserOrganizations
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<UserOrganization>> GetByUserAndOrganizationAsync(
        Guid userId, 
        OrganizationId organizationId, 
        CancellationToken cancellationToken)
    {
        var entity = await context.UserOrganizations
            .AsNoTracking()
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.OrganizationId == organizationId, cancellationToken);

        return entity ?? Option<UserOrganization>.None;
    }
}