using Api.Dtos;
using Application.Analytics.Queries;
using Application.Audit.Queries;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("audit")]
[Authorize(Roles = "OrganizationAdmin,SuperAdmin")]
public class AuditController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Отримати аудіт-логи з фільтрацією.
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult<AuditLogsResultDto>> GetLogs(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery
        {
            OrganizationId = organizationId,
            UserId = userId,
            Action = action,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };

        var result = await sender.Send(query, cancellationToken);

        return new AuditLogsResultDto
        {
            Items = result.Items.Select(a => new AuditLogItemDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                IpAddress = a.IpAddress
            }).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }

    /// <summary>
    /// Отримати аналітику по тесту.
    /// </summary>
    [HttpGet("analytics/test/{testId:guid}")]
    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    public async Task<ActionResult<TestAnalyticsDto>> GetTestAnalytics(
        [FromRoute] Guid testId,
        CancellationToken cancellationToken)
    {
        var query = new GetTestAnalyticsQuery { TestId = testId };
        var result = await sender.Send(query, cancellationToken);

        return new TestAnalyticsDto
        {
            TestId = result.TestId,
            TestTitle = result.TestTitle,
            TotalSessions = result.TotalSessions,
            CompletedSessions = result.CompletedSessions,
            InProgressSessions = result.InProgressSessions,
            AbandonedSessions = result.AbandonedSessions,
            AverageScore = result.AverageScore,
            MinScore = result.MinScore,
            MaxScore = result.MaxScore,
            AverageCompletionTimeMinutes = result.AverageCompletionTimeMinutes,
            TotalViolations = result.TotalViolations,
            ScoreDistribution = result.ScoreDistribution.Select(s => new ScoreDistributionDto
            {
                Range = s.Range,
                Count = s.Count,
                Percentage = s.Percentage
            }).ToList()
        };
    }
}
