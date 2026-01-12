using Domain.Tests;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface ITestSessionRepository
{
    void Add(TestSession entity);
    void Update(TestSession entity);
    Task<Option<TestSession>> GetByIdAsync(TestSessionId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TestSession>> GetByTestIdAsync(TestId testId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TestSession>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TestSession>> GetByTestAndStudentAsync(TestId testId, Guid studentId, CancellationToken cancellationToken);
    Task<Option<TestSession>> GetActiveSessionAsync(TestId testId, Guid studentId, CancellationToken cancellationToken);
    Task<int> GetAttemptCountAsync(TestId testId, Guid studentId, CancellationToken cancellationToken);
}