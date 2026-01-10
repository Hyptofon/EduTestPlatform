using Domain.Common;

namespace Domain.Organizations;

public class OrganizationalUnit : Entity<OrganizationalUnitId>
{
    public required string Name { get; set; }
    public OrganizationalUnitType Type { get; private set; }
    
    public OrganizationalUnitId? ParentId { get; private set; }
    public virtual OrganizationalUnit? Parent { get; private set; }
    
    public virtual ICollection<OrganizationalUnit> Children { get; private set; } = new List<OrganizationalUnit>();

    public string? InviteCode { get; private set; }

    private OrganizationalUnit(OrganizationalUnitId id) : base(id) { }

    public static OrganizationalUnit Create(string name, OrganizationalUnitType type, OrganizationalUnitId? parentId)
    {
        return new OrganizationalUnit(OrganizationalUnitId.New())
        {
            Name = name,
            Type = type,
            ParentId = parentId
        };
    }

    public void SetInviteCode(string code) => InviteCode = code;
    public void AddChild(OrganizationalUnit child) => Children.Add(child);
}