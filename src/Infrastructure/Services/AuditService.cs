using System.Text.Json;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Audit;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

/// <summary>
/// Реалізація сервісу централізованого аудіту.
/// Логує ВСІ важливі дії в системі з контекстом HTTP запиту.
/// </summary>
public class AuditService(
    IAuditLogRepository auditLogRepository,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public async Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        
        var auditLog = AuditLog.Create(
            userId: currentUserService.UserId,
            userEmail: currentUserService.Email ?? "anonymous",
            action: action,
            entityType: entityType,
            entityId: entityId,
            oldValues: oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            newValues: newValues != null ? JsonSerializer.Serialize(newValues) : null,
            ipAddress: GetClientIpAddress(httpContext),
            userAgent: GetUserAgent(httpContext),
            organizationId: organizationId);

        auditLogRepository.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LogAuthenticationAsync(
        string action,
        Guid? userId,
        string userEmail,
        Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        
        var auditLog = AuditLog.Create(
            userId: userId,
            userEmail: userEmail,
            action: action,
            entityType: "User",
            entityId: userId?.ToString(),
            oldValues: null,
            newValues: null,
            ipAddress: GetClientIpAddress(httpContext),
            userAgent: GetUserAgent(httpContext),
            organizationId: organizationId);

        auditLogRepository.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? GetClientIpAddress(HttpContext? context)
    {
        if (context == null) return null;
        
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }
        
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetUserAgent(HttpContext? context)
    {
        return context?.Request.Headers["User-Agent"].FirstOrDefault();
    }
}
