using Application.Common.Interfaces.Repositories;
using Domain.Subjects;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubjectEnrollmentRepository(ApplicationDbContext context) : ISubjectEnrollmentRepository
{
    public void Add(SubjectEnrollment entity)
    {
        context.SubjectEnrollments.Add(entity);
    }

    public void Delete(SubjectEnrollment entity)
    {
        context.SubjectEnrollments.Remove(entity);
    }

    public async Task<Option<SubjectEnrollment>> GetByIdAsync(
        SubjectEnrollmentId id, 
        CancellationToken cancellationToken)
    {
        var entity = await context.SubjectEnrollments
            .AsNoTracking()
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity ?? Option<SubjectEnrollment>.None;
    }

    public async Task<Option<SubjectEnrollment>> GetBySubjectAndUserAsync(
        SubjectId subjectId, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var entity = await context.SubjectEnrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SubjectId == subjectId && x.UserId == userId, cancellationToken);

        return entity ?? Option<SubjectEnrollment>.None;
    }

    public async Task<IReadOnlyList<SubjectEnrollment>> GetBySubjectIdAsync(
        SubjectId subjectId, 
        CancellationToken cancellationToken)
    {
        return await context.SubjectEnrollments
            .AsNoTracking()
            .Where(x => x.SubjectId == subjectId)
            .OrderByDescending(x => x.EnrolledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubjectEnrollment>> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return await context.SubjectEnrollments
            .AsNoTracking()
            .Include(x => x.Subject)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.EnrolledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserEnrolledAsync(
        SubjectId subjectId, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return await context.SubjectEnrollments
            .AsNoTracking()
            .AnyAsync(x => x.SubjectId == subjectId && x.UserId == userId, cancellationToken);
    }
}