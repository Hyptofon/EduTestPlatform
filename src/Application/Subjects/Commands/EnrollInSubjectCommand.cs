using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Subjects.Exceptions;
using Domain.Subjects;
using LanguageExt;
using MediatR;

namespace Application.Subjects.Commands;

public record EnrollInSubjectCommand : IRequest<Either<SubjectException, SubjectEnrollment>>
{
    public required Guid SubjectId { get; init; }
    public required Guid UserId { get; init; }
    public string? AccessKey { get; init; }
}

public class EnrollInSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    ISubjectEnrollmentRepository enrollmentRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<EnrollInSubjectCommand, Either<SubjectException, SubjectEnrollment>>
{
    public async Task<Either<SubjectException, SubjectEnrollment>> Handle(
        EnrollInSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subjectId = new SubjectId(request.SubjectId);
        var subjectOption = await subjectRepository.GetByIdAsync(subjectId, cancellationToken);

        if (subjectOption.IsNone)
        {
            return new SubjectNotFoundException(subjectId);
        }

        var subject = subjectOption.IfNone(() => throw new InvalidOperationException());

        var existingEnrollment = await enrollmentRepository.GetBySubjectAndUserAsync(
            subjectId, 
            request.UserId, 
            cancellationToken);

        if (existingEnrollment.IsSome)
        {
            return new AlreadyEnrolledException(subjectId, request.UserId);
        }

        if (!subject.ValidateAccessKey(request.AccessKey))
        {
            return new InvalidAccessKeyException(subjectId);
        }

        return await CreateEnrollment(subjectId, request.UserId, cancellationToken);
    }

    private async Task<Either<SubjectException, SubjectEnrollment>> CreateEnrollment(
        SubjectId subjectId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var enrollment = SubjectEnrollment.New(subjectId, userId);

            enrollmentRepository.Add(enrollment);
            await dbContext.SaveChangesAsync(cancellationToken);

            return enrollment;
        }
        catch (Exception exception)
        {
            return new UnhandledSubjectException(subjectId, exception);
        }
    }
}