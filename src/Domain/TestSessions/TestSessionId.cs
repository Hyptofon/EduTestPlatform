namespace Domain.TestSessions;

public record TestSessionId(Guid Value)
{
    public static TestSessionId New() => new(Guid.NewGuid());
}