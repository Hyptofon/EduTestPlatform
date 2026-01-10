using Application.Common.Interfaces;
using Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrganizationalUnitRepository(IApplicationDbContext context)
{
    public async Task<OrganizationalUnit?> GetByIdAsync(OrganizationalUnitId id, CancellationToken cancellationToken)
    {
        return await context.OrganizationalUnits
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    
    // Інші методи репозиторію додамо по мірі необхідності
}