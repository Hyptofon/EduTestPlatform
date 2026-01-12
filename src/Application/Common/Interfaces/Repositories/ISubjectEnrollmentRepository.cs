using Domain.Subjects;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface ISubjectEnrollmentRepository
{
    void Add(SubjectEnrollment entity);
    void Delete(SubjectEnrollment entity);
    Task<Option<SubjectEnrollment>> GetByIdAsync(SubjectEnrollmentId id, CancellationToken cancellationToken);
    Task<Option<SubjectEnrollment>> GetBySubjectAndUserAsync(SubjectId subjectId, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubjectEnrollment>> GetBySubjectIdAsync(SubjectId subjectId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubjectEnrollment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsUserEnrolledAsync(SubjectId subjectId, Guid userId, CancellationToken cancellationToken);
}