using Application.Common.Interfaces.Repositories;
using Domain.Subjects;
using LanguageExt;
using MediatR;

namespace Application.Subjects.Queries;

public record GetSubjectByIdQuery(Guid SubjectId) : IRequest<Option<Subject>>;

public class GetSubjectByIdQueryHandler(ISubjectRepository subjectRepository)
    : IRequestHandler<GetSubjectByIdQuery, Option<Subject>>
{
    public async Task<Option<Subject>> Handle(
        GetSubjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        var subjectId = new SubjectId(request.SubjectId);
        return await subjectRepository.GetByIdAsync(subjectId, cancellationToken);
    }
}