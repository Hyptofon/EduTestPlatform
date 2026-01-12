using Domain.Subjects;
using Domain.Tests;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface ITestRepository
{
    void Add(Test entity);
    void Update(Test entity);
    void Delete(Test entity);
    Task<Option<Test>> GetByIdAsync(TestId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Test>> GetBySubjectIdAsync(SubjectId subjectId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Test>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken);
}