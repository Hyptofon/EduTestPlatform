namespace Domain.TestSessions;

public class SessionViolation
{
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty; // "FocusLost", "TabSwitch"
    public string Context { get; set; } = string.Empty; // "Question 5"
    public int DurationSeconds { get; set; }
}