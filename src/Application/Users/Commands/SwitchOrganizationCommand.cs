using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Organizations;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

/// <summary>
/// Command для перемикання активної організації (workspace).
/// Зберігає ID для швидкого доступу при наступному вході.
/// </summary>
public record SwitchOrganizationCommand : IRequest<Either<UserException, ApplicationUser>>
{
    public required Guid OrganizationId { get; init; }
}

public class SwitchOrganizationCommandHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IUserOrganizationRepository userOrganizationRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<SwitchOrganizationCommand, Either<UserException, ApplicationUser>>
{
    public async Task<Either<UserException, ApplicationUser>> Handle(
        SwitchOrganizationCommand request,
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

        // Перевіряємо чи користувач є членом цієї організації
        var organizationId = new OrganizationId(request.OrganizationId);
        var userOrganizations = await userOrganizationRepository.GetByUserIdAsync(userId, cancellationToken);
        
        var isMember = userOrganizations.Any(uo => uo.OrganizationId == organizationId);
        
        if (!isMember)
        {
            return new UserNotInOrganizationException(userId, request.OrganizationId);
        }

        try
        {
            user.SetLastActiveOrganization(organizationId);
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
