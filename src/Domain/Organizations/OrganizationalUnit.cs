using Domain.Common;

namespace Domain.Organizations;

public class OrganizationalUnit : Entity<OrganizationalUnitId>
{
    public required string Name { get; set; }
    public OrganizationalUnitType Type { get; private set; }
    
    public OrganizationalUnitId? ParentId { get; private set; }
    public virtual OrganizationalUnit? Parent { get; private set; }
    
    public virtual ICollection<OrganizationalUnit> Children { get; private set; } = new List<OrganizationalUnit>();

    public string? AccessKey { get; private set; } 

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

    public void AddChild(OrganizationalUnit child) => Children.Add(child);
    
    public bool IsPublic => string.IsNullOrEmpty(AccessKey);

    public void SetAccessKey(string key)
    {
        if (Type != OrganizationalUnitType.Subject)
            throw new InvalidOperationException("Only subjects can have access keys");

        AccessKey = key;
    }

    public void RemoveAccessKey()
    {
        AccessKey = null;
    }

    public bool ValidateAccessKey(string key)
    {
        if (IsPublic) return true;
        return AccessKey == key;
    }
}