using Domain.Audit;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

/// <summary>
/// Репозиторій для роботи з аудіт-логами.
/// </summary>
public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
    
    Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(
        Guid userId, 
        int skip, 
        int take, 
        CancellationToken cancellationToken);
    
    Task<IReadOnlyList<AuditLog>> GetByOrganizationIdAsync(
        Guid organizationId, 
        int skip, 
        int take, 
        CancellationToken cancellationToken);
    
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken);
    
    Task<IReadOnlyList<AuditLog>> GetByActionAsync(
        string action, 
        DateTime? from, 
        DateTime? to, 
        int skip, 
        int take, 
        CancellationToken cancellationToken);
    
    Task<int> GetCountAsync(
        Guid? organizationId, 
        string? action, 
        DateTime? from, 
        DateTime? to, 
        CancellationToken cancellationToken);
}
