using Microsoft.AspNetCore.Identity;

namespace Domain.Users;

public class ApplicationRole : IdentityRole<Guid>
{
    // Global roles
    public const string SuperAdmin = "SuperAdmin";
    
    // Organization roles
    public const string OrganizationAdmin = "OrganizationAdmin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName)
    {
        Id = Guid.NewGuid();
    }
}