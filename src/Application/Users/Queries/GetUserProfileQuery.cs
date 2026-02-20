using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Queries;

/// <summary>
/// Query для отримання повного профілю поточного користувача.
/// Включає базову інформацію та список організацій.
/// </summary>
public record GetUserProfileQuery : IRequest<Either<UserException, UserProfileResult>>;

public record UserProfileResult
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public Guid? LastActiveOrganizationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<UserOrganizationInfo> Organizations { get; init; } = [];
}

public record UserOrganizationInfo
{
    public Guid OrganizationId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid? OrganizationalUnitId { get; init; }
}

public class GetUserProfileQueryHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IUserOrganizationRepository userOrganizationRepository)
    : IRequestHandler<GetUserProfileQuery, Either<UserException, UserProfileResult>>
{
    public async Task<Either<UserException, UserProfileResult>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return new UserNotAuthenticatedException();
        }

        var userId = currentUserService.UserId.Value;
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return new UserNotFoundException(userId);
        }

        var organizations = await userOrganizationRepository
            .GetByUserIdAsync(userId, cancellationToken);

        var orgInfos = organizations.Select(uo => new UserOrganizationInfo
        {
            OrganizationId = uo.OrganizationId.Value,
            OrganizationName = uo.Organization?.Name ?? "Unknown",
            Role = uo.Role,
            OrganizationalUnitId = uo.OrganizationalUnitId?.Value
        }).ToList();

        return new UserProfileResult
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            LastActiveOrganizationId = user.LastActiveOrganizationId?.Value,
            CreatedAt = user.CreatedAt,
            Organizations = orgInfos
        };
    }
}
