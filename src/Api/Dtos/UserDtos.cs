using Domain.Users;

namespace Api.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    bool IsActive)
{
    public static UserDto FromDomainModel(ApplicationUser user)
        => new(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            !user.IsBlocked); 
}

public record UpdateUserProfileDto(
    string FirstName,
    string LastName);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword);
    
public record UserOrganizationDto(
    Guid UserId,
    Guid OrganizationId,
    string Role,
    DateTime JoinedAt)
{
    public static UserOrganizationDto FromDomainModel(UserOrganization userOrg)
        => new(
            userOrg.UserId,
            userOrg.OrganizationId.Value,
            userOrg.Role,
            userOrg.JoinedAt);
}