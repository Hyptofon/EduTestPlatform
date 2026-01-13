using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using Domain.Subjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubjectRepository(ApplicationDbContext context) : ISubjectRepository
{
    public void Add(Subject entity)
    {
        context.Subjects.Add(entity);
    }

    public void Update(Subject entity)
    {
        context.Subjects.Update(entity);
    }

    public void Delete(Subject entity)
    {
        context.Subjects.Remove(entity);
    }

    public async Task<Option<Subject>> GetByIdAsync(SubjectId id, CancellationToken cancellationToken)
    {
        var entity = await context.Subjects
            .AsNoTracking()
            .Include(x => x.OrganizationalUnit)
            .Include(x => x.Enrollments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<Subject>.None;
    }

    public async Task<IReadOnlyList<Subject>> GetByOrganizationalUnitIdAsync(
        OrganizationalUnitId unitId, 
        CancellationToken cancellationToken)
    {
        return await context.Subjects
            .AsNoTracking()
            .Where(x => x.OrganizationalUnitId == unitId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subject>> GetByTeacherIdAsync(
        Guid teacherId, 
        CancellationToken cancellationToken)
    {
        return await context.Subjects
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == teacherId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}