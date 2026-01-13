using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrganizationalUnitRepository(ApplicationDbContext context) : IOrganizationalUnitRepository
{
    public void Add(OrganizationalUnit entity)
    {
        context.OrganizationalUnits.Add(entity);
    }

    public void Update(OrganizationalUnit entity)
    {
        context.OrganizationalUnits.Update(entity);
    }

    public void Delete(OrganizationalUnit entity)
    {
        context.OrganizationalUnits.Remove(entity);
    }

    public async Task<Option<OrganizationalUnit>> GetByIdAsync(
        OrganizationalUnitId id, 
        CancellationToken cancellationToken)
    {
        var entity = await context.OrganizationalUnits
            .AsNoTracking()
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<OrganizationalUnit>.None;
    }

    public async Task<IReadOnlyList<OrganizationalUnit>> GetByOrganizationIdAsync(
        OrganizationId organizationId, 
        CancellationToken cancellationToken)
    {
        return await context.OrganizationalUnits
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationalUnit>> GetByParentIdAsync(
        OrganizationalUnitId parentId, 
        CancellationToken cancellationToken)
    {
        return await context.OrganizationalUnits
            .AsNoTracking()
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationalUnit>> GetRootUnitsAsync(
        OrganizationId organizationId, 
        CancellationToken cancellationToken)
    {
        return await context.OrganizationalUnits
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.ParentId == null)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}