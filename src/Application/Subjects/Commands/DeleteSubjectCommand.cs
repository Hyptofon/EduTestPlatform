using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Subjects.Exceptions;
using Domain.Subjects;
using LanguageExt;
using MediatR;

namespace Application.Subjects.Commands;

public record DeleteSubjectCommand(Guid SubjectId)
    : IRequest<Either<SubjectException, Subject>>;

public class DeleteSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteSubjectCommand, Either<SubjectException, Subject>>
{
    public async Task<Either<SubjectException, Subject>> Handle(
        DeleteSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subjectId = new SubjectId(request.SubjectId);
        var existingSubject = await subjectRepository.GetByIdAsync(subjectId, cancellationToken);

        return await existingSubject.MatchAsync(
            subject => DeleteEntity(subject, cancellationToken),
            () => Task.FromResult<Either<SubjectException, Subject>>(
                new SubjectNotFoundException(subjectId)));
    }

    private async Task<Either<SubjectException, Subject>> DeleteEntity(
        Subject subject,
        CancellationToken cancellationToken)
    {
        try
        {
            subjectRepository.Delete(subject);
            await dbContext.SaveChangesAsync(cancellationToken);

            return subject;
        }
        catch (Exception exception)
        {
            return new UnhandledSubjectException(subject.Id, exception);
        }
    }
}