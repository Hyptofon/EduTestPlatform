namespace Domain.Tests;

public class Violation
{
    public DateTime Timestamp { get; private set; }
    public string Type { get; private set; }
    public Guid? QuestionId { get; private set; }
    public int DurationSeconds { get; private set; }

    private Violation(
        DateTime timestamp,
        string type,
        Guid? questionId,
        int durationSeconds)
    {
        Timestamp = timestamp;
        Type = type;
        QuestionId = questionId;
        DurationSeconds = durationSeconds;
    }

    public static Violation FocusLost(Guid? questionId, int durationSeconds)
    {
        return new Violation(
            DateTime.UtcNow,
            "FocusLost",
            questionId,
            durationSeconds);
    }

    public static Violation TabSwitch(Guid? questionId)
    {
        return new Violation(
            DateTime.UtcNow,
            "TabSwitch",
            questionId,
            0);
    }
}