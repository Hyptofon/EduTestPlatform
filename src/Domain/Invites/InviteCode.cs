using Domain.Organizations;

namespace Domain.Invites;

public class InviteCode
{
    public InviteCodeId Id { get; }
    public OrganizationId OrganizationId { get; }
    public OrganizationalUnitId? OrganizationalUnitId { get; private set; }
    public string Code { get; private set; }
    public InviteCodeType Type { get; private set; }
    public string AssignedRole { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? ExpiresAt { get; private set; }
    public int? MaxUses { get; private set; }
    public int CurrentUses { get; private set; }

    public Organization? Organization { get; private set; }
    public OrganizationalUnit? OrganizationalUnit { get; private set; }

    private InviteCode(
        InviteCodeId id,
        OrganizationId organizationId,
        OrganizationalUnitId? organizationalUnitId,
        string code,
        InviteCodeType type,
        string assignedRole,
        bool isActive,
        DateTime createdAt,
        DateTime? expiresAt,
        int? maxUses,
        int currentUses)
    {
        Id = id;
        OrganizationId = organizationId;
        OrganizationalUnitId = organizationalUnitId;
        Code = code;
        Type = type;
        AssignedRole = assignedRole;
        IsActive = isActive;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        MaxUses = maxUses;
        CurrentUses = currentUses;
    }

    public static InviteCode New(
        InviteCodeId id,
        OrganizationId organizationId,
        OrganizationalUnitId? organizationalUnitId,
        string code,
        InviteCodeType type,
        string assignedRole,
        DateTime? expiresAt = null,
        int? maxUses = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Invite code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(assignedRole))
            throw new ArgumentException("Assigned role cannot be empty", nameof(assignedRole));

        return new InviteCode(
            id,
            organizationId,
            organizationalUnitId,
            code,
            type,
            assignedRole,
            true,
            DateTime.UtcNow,
            expiresAt,
            maxUses,
            0);
    }

    public void Use()
    {
        if (!IsActive)
            throw new InvalidOperationException("Invite code is not active");

        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException("Invite code has expired");

        if (MaxUses.HasValue && CurrentUses >= MaxUses.Value)
            throw new InvalidOperationException("Invite code has reached maximum uses");

        CurrentUses++;

        if (MaxUses.HasValue && CurrentUses >= MaxUses.Value)
        {
            IsActive = false;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsValid()
    {
        if (!IsActive) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow) return false;
        if (MaxUses.HasValue && CurrentUses >= MaxUses.Value) return false;
        return true;
    }
}