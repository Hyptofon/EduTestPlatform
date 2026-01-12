namespace Domain.Tests;

public record TestId(Guid Value)
{
    public static TestId Empty() => new(Guid.Empty);
    public static TestId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}