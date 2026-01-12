namespace Domain.Tests;

public record TestSessionId(Guid Value)
{
    public static TestSessionId Empty() => new(Guid.Empty);
    public static TestSessionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}