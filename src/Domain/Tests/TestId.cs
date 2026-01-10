namespace Domain.Tests;

public record TestId(Guid Value)
{
    public static TestId New() => new(Guid.NewGuid());
}