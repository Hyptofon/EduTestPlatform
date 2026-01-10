using Microsoft.AspNetCore.Identity;

namespace Domain.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public virtual ICollection<UserOrganization> Organizations { get; set; } = new List<UserOrganization>();
}