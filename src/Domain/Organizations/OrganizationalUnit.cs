namespace Domain.Organizations;

public class OrganizationalUnit
{
    public OrganizationalUnitId Id { get; }
    public OrganizationId OrganizationId { get; }
    public OrganizationalUnitId? ParentId { get; private set; }
    public OrganizationalUnitType Type { get; private set; }
    public string Name { get; private set; }
    public string? Settings { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public Organization? Organization { get; private set; }
    public OrganizationalUnit? Parent { get; private set; }
    public ICollection<OrganizationalUnit>? Children { get; private set; } = [];

    private OrganizationalUnit(
        OrganizationalUnitId id,
        OrganizationId organizationId,
        OrganizationalUnitId? parentId,
        OrganizationalUnitType type,
        string name,
        string? settings,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Id = id;
        OrganizationId = organizationId;
        ParentId = parentId;
        Type = type;
        Name = name;
        Settings = settings;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static OrganizationalUnit New(
        OrganizationalUnitId id,
        OrganizationId organizationId,
        OrganizationalUnitId? parentId,
        OrganizationalUnitType type,
        string name,
        string? settings = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Unit name cannot be empty", nameof(name));

        return new OrganizationalUnit(
            id,
            organizationId,
            parentId,
            type,
            name,
            settings,
            DateTime.UtcNow,
            null);
    }

    public void UpdateDetails(string name, string? settings)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Unit name cannot be empty", nameof(name));

        Name = name;
        Settings = settings;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeParent(OrganizationalUnitId? newParentId)
    {
        ParentId = newParentId;
        UpdatedAt = DateTime.UtcNow;
    }
}