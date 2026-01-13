using Application.Common.Interfaces.Repositories;
using Domain.Tests;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TestSessionRepository(ApplicationDbContext context) : ITestSessionRepository
{
    public void Add(TestSession entity)
    {
        context.TestSessions.Add(entity);
    }

    public void Update(TestSession entity)
    {
        context.TestSessions.Update(entity);
    }

    public async Task<Option<TestSession>> GetByIdAsync(TestSessionId id, CancellationToken cancellationToken)
    {
        var entity = await context.TestSessions
            .AsNoTracking()
            .Include(x => x.Test)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<TestSession>.None;
    }

    public async Task<IReadOnlyList<TestSession>> GetByTestIdAsync(
        TestId testId, 
        CancellationToken cancellationToken)
    {
        return await context.TestSessions
            .AsNoTracking()
            .Where(x => x.TestId == testId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestSession>> GetByStudentIdAsync(
        Guid studentId, 
        CancellationToken cancellationToken)
    {
        return await context.TestSessions
            .AsNoTracking()
            .Include(x => x.Test)
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestSession>> GetByTestAndStudentAsync(
        TestId testId, 
        Guid studentId, 
        CancellationToken cancellationToken)
    {
        return await context.TestSessions
            .AsNoTracking()
            .Where(x => x.TestId == testId && x.StudentId == studentId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<TestSession>> GetActiveSessionAsync(
        TestId testId, 
        Guid studentId, 
        CancellationToken cancellationToken)
    {
        var entity = await context.TestSessions
            .FirstOrDefaultAsync(
                x => x.TestId == testId && 
                     x.StudentId == studentId && 
                     x.Status == TestSessionStatus.InProgress, 
                cancellationToken);

        return entity ?? Option<TestSession>.None;
    }

    public async Task<int> GetAttemptCountAsync(
        TestId testId, 
        Guid studentId, 
        CancellationToken cancellationToken)
    {
        return await context.TestSessions
            .AsNoTracking()
            .CountAsync(
                x => x.TestId == testId && 
                     x.StudentId == studentId && 
                     x.Status == TestSessionStatus.Completed, 
                cancellationToken);
    }
}