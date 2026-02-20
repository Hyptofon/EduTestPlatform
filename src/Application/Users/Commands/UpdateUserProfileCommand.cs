using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

/// <summary>
/// Command для оновлення профілю користувача.
/// Можна оновити ім'я, прізвище та аватар.
/// </summary>
public record UpdateUserProfileCommand : IRequest<Either<UserException, ApplicationUser>>
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? AvatarUrl { get; init; }
}

public class UpdateUserProfileCommandHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateUserProfileCommand, Either<UserException, ApplicationUser>>
{
    public async Task<Either<UserException, ApplicationUser>> Handle(
        UpdateUserProfileCommand request,
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

        try
        {
            // Оновлюємо тільки надані поля
            if (!string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName))
            {
                var firstName = request.FirstName ?? user.FirstName;
                var lastName = request.LastName ?? user.LastName;
                user.UpdateProfile(firstName, lastName);
            }

            if (request.AvatarUrl != null)
            {
                user.UpdateAvatar(request.AvatarUrl);
            }

            await userManager.UpdateAsync(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return user;
        }
        catch (Exception ex)
        {
            return new UnhandledUserException(userId, ex);
        }
    }
}
