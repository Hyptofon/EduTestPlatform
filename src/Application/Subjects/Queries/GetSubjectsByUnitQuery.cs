using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using Domain.Subjects;
using MediatR;

namespace Application.Subjects.Queries;

public record GetSubjectsByUnitQuery(Guid UnitId) : IRequest<IReadOnlyList<Subject>>;

public class GetSubjectsByUnitQueryHandler(ISubjectRepository subjectRepository)
    : IRequestHandler<GetSubjectsByUnitQuery, IReadOnlyList<Subject>>
{
    public async Task<IReadOnlyList<Subject>> Handle(
        GetSubjectsByUnitQuery request,
        CancellationToken cancellationToken)
    {
        var unitId = new OrganizationalUnitId(request.UnitId);
        return await subjectRepository.GetByOrganizationalUnitIdAsync(unitId, cancellationToken);
    }
}