using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrganizationRepository(ApplicationDbContext context) : IOrganizationRepository
{
    public void Add(Organization entity)
    {
        context.Organizations.Add(entity);
    }

    public void Update(Organization entity)
    {
        context.Organizations.Update(entity);
    }

    public void Delete(Organization entity)
    {
        context.Organizations.Remove(entity);
    }

    public async Task<Option<Organization>> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken)
    {
        var entity = await context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<Organization>.None;
    }

    public async Task<Option<Organization>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

        return entity ?? Option<Organization>.None;
    }

    public async Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Organizations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}