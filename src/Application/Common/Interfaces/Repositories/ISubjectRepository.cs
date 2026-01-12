using Domain.Organizations;
using Domain.Subjects;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface ISubjectRepository
{
    void Add(Subject entity);
    void Update(Subject entity);
    void Delete(Subject entity);
    Task<Option<Subject>> GetByIdAsync(SubjectId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Subject>> GetByOrganizationalUnitIdAsync(OrganizationalUnitId unitId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Subject>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken);
}