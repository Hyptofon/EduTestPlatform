using Domain.Common;
using Domain.Organizations;

namespace Domain.Users;

public record InviteCodeId(Guid Value)
{
    public static InviteCodeId New() => new(Guid.NewGuid());
    public static InviteCodeId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public class InviteCode : Entity<InviteCodeId>
{
    public string Code { get; private set; }
    public UserRole TargetRole { get; private set; }
    public OrganizationalUnitId OrganizationalUnitId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public bool IsUsed { get; private set; }
    public int MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    
    public virtual OrganizationalUnit? OrganizationalUnit { get; private set; }

    private InviteCode(
        InviteCodeId id, 
        string code, 
        UserRole targetRole, 
        OrganizationalUnitId organizationalUnitId,
        DateTime createdAt,
        DateTime? expiryDate,
        int maxUses) : base(id)
    {
        Code = code;
        TargetRole = targetRole;
        OrganizationalUnitId = organizationalUnitId;
        CreatedAt = createdAt;
        ExpiryDate = expiryDate;
        MaxUses = maxUses;
        IsUsed = false;
        UsedCount = 0;
    }

    public static InviteCode Generate(
        string code, 
        UserRole targetRole, 
        OrganizationalUnitId organizationalUnitId,
        DateTime? expiryDate = null,
        int maxUses = 1)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Invite code cannot be empty", nameof(code));

        return new InviteCode(
            InviteCodeId.New(),
            code.ToUpperInvariant(),
            targetRole,
            organizationalUnitId,
            DateTime.UtcNow,
            expiryDate,
            maxUses);
    }

    public bool IsValid()
    {
        if (IsUsed && MaxUses == 1)
            return false;

        if (UsedCount >= MaxUses)
            return false;

        if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow)
            return false;

        return true;
    }

    public void MarkAsUsed()
    {
        UsedCount++;
        
        if (UsedCount >= MaxUses)
        {
            IsUsed = true;
        }
    }

    public void Revoke()
    {
        IsUsed = true;
    }
}