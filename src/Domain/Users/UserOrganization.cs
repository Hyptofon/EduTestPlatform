using Domain.Organizations;

namespace Domain.Users;

public class UserOrganization
{
    public UserOrganizationId Id { get; }
    public Guid UserId { get; }
    public OrganizationId OrganizationId { get; }
    public string Role { get; private set; }
    public DateTime JoinedAt { get; }

    public ApplicationUser? User { get; private set; }
    public Organization? Organization { get; private set; }

    private UserOrganization(
        UserOrganizationId id,
        Guid userId,
        OrganizationId organizationId,
        string role,
        DateTime joinedAt)
    {
        Id = id;
        UserId = userId;
        OrganizationId = organizationId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public static UserOrganization New(
        Guid userId,
        OrganizationId organizationId,
        string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty", nameof(role));

        return new UserOrganization(
            UserOrganizationId.New(),
            userId,
            organizationId,
            role,
            DateTime.UtcNow);
    }

    public void UpdateRole(string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
            throw new ArgumentException("Role cannot be empty", nameof(newRole));

        Role = newRole;
    }
}