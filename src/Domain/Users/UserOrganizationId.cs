namespace Domain.Users;

public record UserOrganizationId(Guid Value)
{
    public static UserOrganizationId Empty() => new(Guid.Empty);
    public static UserOrganizationId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}