namespace Domain.Organizations;

public record OrganizationId(Guid Value)
{
    public static OrganizationId Empty() => new(Guid.Empty);
    public static OrganizationId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}