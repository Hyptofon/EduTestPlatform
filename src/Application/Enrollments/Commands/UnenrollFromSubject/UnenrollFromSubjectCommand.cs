using Application.Common.Interfaces;
using Domain.Enrollments;
using Domain.Organizations;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Enrollments.Commands.UnenrollFromSubject;

public record UnenrollFromSubjectCommand : IRequest<Either<Exception, StudentSubject>>
{
    public required Guid SubjectId { get; init; }
}

public class UnenrollFromSubjectCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<UnenrollFromSubjectCommand, Either<Exception, StudentSubject>>
{
    public async Task<Either<Exception, StudentSubject>> Handle(
        UnenrollFromSubjectCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                return new UnauthorizedAccessException("User must be authenticated");
            }

            var userId = currentUserService.UserId.Value;
            var subjectId = new OrganizationalUnitId(request.SubjectId);

            var enrollment = await context.StudentSubjects
                .FirstOrDefaultAsync(
                    x => x.StudentId == userId && x.SubjectId == subjectId && x.IsActive,
                    cancellationToken);

            if (enrollment == null)
            {
                return new KeyNotFoundException("Enrollment not found or already inactive");
            }

            enrollment.Unenroll();
            context.StudentSubjects.Update(enrollment);
            await context.SaveChangesAsync(cancellationToken);

            return enrollment;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}