using Application.Common.Interfaces.Repositories;
using Domain.Audit;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Реалізація репозиторію аудіт-логів.
/// </summary>
public class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    public void Add(AuditLog auditLog)
    {
        context.AuditLogs.Add(auditLog);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(
        Guid userId, 
        int skip, 
        int take, 
        CancellationToken cancellationToken)
    {
        return await context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByOrganizationIdAsync(
        Guid organizationId, 
        int skip, 
        int take, 
        CancellationToken cancellationToken)
    {
        return await context.AuditLogs
            .Where(a => a.OrganizationId == organizationId)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken)
    {
        return await context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByActionAsync(
        string action, 
        DateTime? from, 
        DateTime? to, 
        int skip, 
        int take, 
        CancellationToken cancellationToken)
    {
        var query = context.AuditLogs.Where(a => a.Action == action);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(
        Guid? organizationId, 
        string? action, 
        DateTime? from, 
        DateTime? to, 
        CancellationToken cancellationToken)
    {
        var query = context.AuditLogs.AsQueryable();

        if (organizationId.HasValue)
            query = query.Where(a => a.OrganizationId == organizationId);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.CountAsync(cancellationToken);
    }
}
