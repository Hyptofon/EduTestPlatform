using MediatR;

namespace Application.TestSessions.Events;

/// <summary>
/// Подія початку тестової сесії.
/// </summary>
public record TestSessionStartedEvent : INotification
{
    public Guid SessionId { get; init; }
    public Guid TestId { get; init; }
    public Guid UserId { get; init; }
    public DateTime StartedAt { get; init; }
}

/// <summary>
/// Подія подання відповіді.
/// </summary>
public record AnswerSubmittedEvent : INotification
{
    public Guid SessionId { get; init; }
    public Guid TestId { get; init; }
    public Guid UserId { get; init; }
    public Guid QuestionId { get; init; }
    public int QuestionNumber { get; init; }
    public int TotalQuestions { get; init; }
    public DateTime AnsweredAt { get; init; }
}

/// <summary>
/// Подія порушення.
/// </summary>
public record ViolationRecordedEvent : INotification
{
    public Guid SessionId { get; init; }
    public Guid TestId { get; init; }
    public Guid UserId { get; init; }
    public string ViolationType { get; init; } = string.Empty;
    public int TotalViolations { get; init; }
    public DateTime RecordedAt { get; init; }
}

/// <summary>
/// Подія завершення сесії.
/// </summary>
public record TestSessionCompletedEvent : INotification
{
    public Guid SessionId { get; init; }
    public Guid TestId { get; init; }
    public Guid UserId { get; init; }
    public decimal? Score { get; init; }
    public decimal? MaxScore { get; init; }
    public int ViolationCount { get; init; }
    public DateTime CompletedAt { get; init; }
}
