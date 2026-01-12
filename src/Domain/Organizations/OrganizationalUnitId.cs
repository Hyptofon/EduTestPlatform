namespace Domain.Organizations;

public record OrganizationalUnitId(Guid Value)
{
    public static OrganizationalUnitId Empty() => new(Guid.Empty);
    public static OrganizationalUnitId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}