using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Subjects.Exceptions;
using Domain.Subjects;
using LanguageExt;
using MediatR;

namespace Application.Subjects.Commands;

public record UpdateSubjectCommand : IRequest<Either<SubjectException, Subject>>
{
    public required Guid SubjectId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required SubjectAccessType AccessType { get; init; }
    public string? AccessKey { get; init; }
}

public class UpdateSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateSubjectCommand, Either<SubjectException, Subject>>
{
    public async Task<Either<SubjectException, Subject>> Handle(
        UpdateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subjectId = new SubjectId(request.SubjectId);
        var existingSubject = await subjectRepository.GetByIdAsync(subjectId, cancellationToken);

        return await existingSubject.MatchAsync(
            subject => UpdateEntity(subject, request, cancellationToken),
            () => Task.FromResult<Either<SubjectException, Subject>>(
                new SubjectNotFoundException(subjectId)));
    }

    private async Task<Either<SubjectException, Subject>> UpdateEntity(
        Subject subject,
        UpdateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            subject.UpdateDetails(request.Name, request.Description);
            subject.UpdateAccessType(request.AccessType, request.AccessKey);

            subjectRepository.Update(subject);
            await dbContext.SaveChangesAsync(cancellationToken);

            return subject;
        }
        catch (Exception exception)
        {
            return new UnhandledSubjectException(subject.Id, exception);
        }
    }
}