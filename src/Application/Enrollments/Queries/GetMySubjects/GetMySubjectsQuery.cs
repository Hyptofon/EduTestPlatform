using Application.Common.Interfaces;
using Domain.Enrollments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Enrollments.Queries.GetMySubjects;

public record GetMySubjectsQuery : IRequest<IReadOnlyList<StudentSubject>>;

public class GetMySubjectsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetMySubjectsQuery, IReadOnlyList<StudentSubject>>
{
    public async Task<IReadOnlyList<StudentSubject>> Handle(
        GetMySubjectsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            return Array.Empty<StudentSubject>();
        }

        var userId = currentUserService.UserId.Value;

        return await context.StudentSubjects
            .Include(x => x.Subject)
            .Where(x => x.StudentId == userId && x.IsActive)
            .OrderByDescending(x => x.EnrolledAt)
            .ToListAsync(cancellationToken);
    }
}