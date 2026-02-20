namespace Api.Dtos;

/// <summary>
/// DTOs для аудіту та аналітики.
/// </summary>

public record AuditLogsResultDto
{
    public IReadOnlyList<AuditLogItemDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public record AuditLogItemDto
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

public record TestAnalyticsDto
{
    public Guid TestId { get; init; }
    public string TestTitle { get; init; } = string.Empty;
    public int TotalSessions { get; init; }
    public int CompletedSessions { get; init; }
    public int InProgressSessions { get; init; }
    public int AbandonedSessions { get; init; }
    public decimal AverageScore { get; init; }
    public decimal MinScore { get; init; }
    public decimal MaxScore { get; init; }
    public decimal AverageCompletionTimeMinutes { get; init; }
    public int TotalViolations { get; init; }
    public IReadOnlyList<ScoreDistributionDto> ScoreDistribution { get; init; } = [];
}

public record ScoreDistributionDto
{
    public string Range { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}
