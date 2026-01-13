using Application.Common.Interfaces.Repositories;
using Domain.Subjects;
using Domain.Tests;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TestRepository(ApplicationDbContext context) : ITestRepository
{
    public void Add(Test entity)
    {
        context.Tests.Add(entity);
    }

    public void Update(Test entity)
    {
        context.Tests.Update(entity);
    }

    public void Delete(Test entity)
    {
        context.Tests.Remove(entity);
    }

    public async Task<Option<Test>> GetByIdAsync(TestId id, CancellationToken cancellationToken)
    {
        var entity = await context.Tests
            .AsNoTracking()
            .Include(x => x.Subject)
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<Test>.None;
    }

    public async Task<IReadOnlyList<Test>> GetBySubjectIdAsync(
        SubjectId subjectId, 
        CancellationToken cancellationToken)
    {
        return await context.Tests
            .AsNoTracking()
            .Where(x => x.SubjectId == subjectId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Test>> GetByTeacherIdAsync(
        Guid teacherId, 
        CancellationToken cancellationToken)
    {
        return await context.Tests
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == teacherId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}