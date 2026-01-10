using Domain.Organizations;

namespace Domain.Users;

public class UserOrganization
{
    public Guid UserId { get; set; }
    public required OrganizationalUnitId OrganizationalUnitId { get; set; }
    public UserRole Role { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual OrganizationalUnit OrganizationalUnit { get; set; } = null!;
}