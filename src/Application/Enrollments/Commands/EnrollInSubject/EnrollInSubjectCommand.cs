using Application.Common.Interfaces;
using Domain.Enrollments;
using Domain.Organizations;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Enrollments.Commands.EnrollInSubject;

public record EnrollInSubjectCommand : IRequest<Either<Exception, StudentSubject>>
{
    public required Guid SubjectId { get; init; }
    public string? AccessKey { get; init; } // null для публічних предметів
}

public class EnrollInSubjectCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<EnrollInSubjectCommand, Either<Exception, StudentSubject>>
{
    public async Task<Either<Exception, StudentSubject>> Handle(
        EnrollInSubjectCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Перевірка авторизації
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                return new UnauthorizedAccessException("User must be authenticated");
            }

            var userId = currentUserService.UserId.Value;
            var subjectId = new OrganizationalUnitId(request.SubjectId);

            // 2. Перевірка, що це Subject
            var subject = await context.OrganizationalUnits
                .FirstOrDefaultAsync(x => x.Id == subjectId, cancellationToken);

            if (subject == null)
            {
                return new KeyNotFoundException($"Subject with ID {request.SubjectId} not found");
            }

            if (subject.Type != OrganizationalUnitType.Subject)
            {
                return new InvalidOperationException("Can only enroll in Subjects");
            }

            // 3. Перевірка AccessKey для приватних предметів
            if (!subject.IsPublic)
            {
                if (string.IsNullOrEmpty(request.AccessKey))
                {
                    return new UnauthorizedAccessException("Access key is required for private subject");
                }

                if (!subject.ValidateAccessKey(request.AccessKey))
                {
                    return new UnauthorizedAccessException("Invalid access key");
                }
            }

            // 4. Перевірка, чи вже записаний
            var existingEnrollment = await context.StudentSubjects
                .FirstOrDefaultAsync(x => x.StudentId == userId && x.SubjectId == subjectId, cancellationToken);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsActive)
                {
                    return new InvalidOperationException("Already enrolled in this subject");
                }

                // Реактивуємо, якщо було відписано
                existingEnrollment.Reenroll();
                context.StudentSubjects.Update(existingEnrollment);
                await context.SaveChangesAsync(cancellationToken);
                return existingEnrollment;
            }

            // 5. Створення запису
            var enrollment = StudentSubject.Enroll(userId, subjectId);

            context.StudentSubjects.Add(enrollment);
            await context.SaveChangesAsync(cancellationToken);

            return enrollment;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}