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

/// <summary>
/// DTO профілю користувача з організаціями.
/// </summary>
public record UserProfileDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public Guid? LastActiveOrganizationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<UserOrganizationInfoDto> Organizations { get; init; } = [];
}

/// <summary>
/// DTO організації користувача (розширений).
/// </summary>
public record UserOrganizationInfoDto
{
    public Guid OrganizationId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid? OrganizationalUnitId { get; init; }
}

public record UpdateUserProfileDto(
    string? FirstName,
    string? LastName,
    string? AvatarUrl);

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

/// <summary>
/// DTO для перемикання організації.
/// </summary>
public record SwitchOrganizationDto(Guid OrganizationId);

/// <summary>
/// DTO для приєднання до організації.
/// </summary>
public record JoinOrganizationDto(string InviteCode);