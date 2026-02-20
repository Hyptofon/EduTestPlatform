using Application.Common.Interfaces.Repositories;
using Domain.Audit;
using LanguageExt;
using MediatR;

namespace Application.Audit.Queries;

/// <summary>
/// Query для отримання аудіт-логів з фільтрацією та пагінацією.
/// </summary>
public record GetAuditLogsQuery : IRequest<AuditLogsResult>
{
    public Guid? OrganizationId { get; init; }
    public Guid? UserId { get; init; }
    public string? Action { get; init; }
    public string? EntityType { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record AuditLogsResult
{
    public IReadOnlyList<AuditLogDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public record AuditLogDto
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
    public Guid? UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string? IpAddress { get; init; }
}

public class GetAuditLogsQueryHandler(
    IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditLogsQuery, AuditLogsResult>
{
    public async Task<AuditLogsResult> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        
        IReadOnlyList<AuditLog> logs;

        if (request.UserId.HasValue)
        {
            logs = await auditLogRepository.GetByUserIdAsync(
                request.UserId.Value, 
                skip, 
                request.PageSize, 
                cancellationToken);
        }
        else if (request.OrganizationId.HasValue)
        {
            logs = await auditLogRepository.GetByOrganizationIdAsync(
                request.OrganizationId.Value, 
                skip, 
                request.PageSize, 
                cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.Action))
        {
            logs = await auditLogRepository.GetByActionAsync(
                request.Action,
                request.From,
                request.To,
                skip,
                request.PageSize,
                cancellationToken);
        }
        else
        {
            logs = [];
        }

        var totalCount = await auditLogRepository.GetCountAsync(
            request.OrganizationId,
            request.Action,
            request.From,
            request.To,
            cancellationToken);

        return new AuditLogsResult
        {
            Items = logs.Select(l => new AuditLogDto
            {
                Id = l.Id.Value,
                Timestamp = l.Timestamp,
                UserId = l.UserId,
                UserEmail = l.UserEmail,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                IpAddress = l.IpAddress
            }).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
