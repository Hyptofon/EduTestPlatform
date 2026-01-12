using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.OrganizationalUnits.Exceptions;
using Application.Subjects.Exceptions;
using Domain.Organizations;
using Domain.Subjects;
using LanguageExt;
using MediatR;

namespace Application.Subjects.Commands;

public record CreateSubjectCommand : IRequest<Either<SubjectException, Subject>>
{
    public required Guid OrganizationalUnitId { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required SubjectAccessType AccessType { get; init; }
    public string? AccessKey { get; init; }
}

public class CreateSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    IOrganizationalUnitRepository unitRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateSubjectCommand, Either<SubjectException, Subject>>
{
    public async Task<Either<SubjectException, Subject>> Handle(
        CreateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var unitId = new OrganizationalUnitId(request.OrganizationalUnitId);
        var unitExists = await unitRepository.GetByIdAsync(unitId, cancellationToken);

        if (unitExists.IsNone)
        {
            return new SubjectAccessDeniedException(SubjectId.Empty(), "Organizational unit not found");
        }

        return await CreateEntity(request, unitId, cancellationToken);
    }

    private async Task<Either<SubjectException, Subject>> CreateEntity(
        CreateSubjectCommand request,
        OrganizationalUnitId unitId,
        CancellationToken cancellationToken)
    {
        try
        {
            var subject = Subject.New(
                SubjectId.New(),
                unitId,
                request.CreatedByUserId,
                request.Name,
                request.Description,
                request.AccessType,
                request.AccessKey);

            subjectRepository.Add(subject);
            await dbContext.SaveChangesAsync(cancellationToken);

            return subject;
        }
        catch (Exception exception)
        {
            return new UnhandledSubjectException(SubjectId.Empty(), exception);
        }
    }
}