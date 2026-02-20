namespace Api.Hubs;

/// <summary>
/// Інтерфейс клієнта для SignalR hub тестових сесій.
/// Визначає методи які можуть бути викликані на клієнті.
/// </summary>
public interface ITestSessionHubClient
{
    /// <summary>
    /// Сповіщає про початок нової тестової сесії студентом.
    /// </summary>
    Task SessionStarted(SessionStartedMessage message);
    
    /// <summary>
    /// Сповіщає про подання відповіді студентом.
    /// </summary>
    Task AnswerSubmitted(AnswerSubmittedMessage message);
    
    /// <summary>
    /// Сповіщає про порушення (втрата фокусу, зміна вкладки тощо).
    /// </summary>
    Task ViolationRecorded(ViolationRecordedMessage message);
    
    /// <summary>
    /// Сповіщає про завершення тестової сесії.
    /// </summary>
    Task SessionCompleted(SessionCompletedMessage message);
    
    /// <summary>
    /// Оновлює статистику прогресу в реальному часі.
    /// </summary>
    Task ProgressUpdated(ProgressUpdatedMessage message);
}

// Повідомлення для SignalR

public record SessionStartedMessage
{
    public Guid SessionId { get; init; }
    public Guid TestId { get; init; }
    public Guid UserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
}

public record AnswerSubmittedMessage
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public Guid QuestionId { get; init; }
    public int QuestionNumber { get; init; }
    public int TotalQuestions { get; init; }
    public DateTime AnsweredAt { get; init; }
}

public record ViolationRecordedMessage
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string ViolationType { get; init; } = string.Empty;
    public int TotalViolations { get; init; }
    public DateTime RecordedAt { get; init; }
}

public record SessionCompletedMessage
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public decimal? Score { get; init; }
    public decimal? MaxScore { get; init; }
    public int ViolationCount { get; init; }
    public DateTime CompletedAt { get; init; }
}

public record ProgressUpdatedMessage
{
    public Guid TestId { get; init; }
    public int ActiveSessions { get; init; }
    public int CompletedSessions { get; init; }
    public int TotalViolations { get; init; }
    public decimal AverageProgress { get; init; }
}
