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
/// Command для приєднання існуючого користувача до нової організації через інвайт-код.
/// </summary>
public record JoinOrganizationCommand : IRequest<Either<UserException, UserOrganization>>
{
    public required string InviteCode { get; init; }
}

public class JoinOrganizationCommandHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IInviteCodeRepository inviteCodeRepository,
    IUserOrganizationRepository userOrganizationRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<JoinOrganizationCommand, Either<UserException, UserOrganization>>
{
    public async Task<Either<UserException, UserOrganization>> Handle(
        JoinOrganizationCommand request,
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

        // Перевіряємо інвайт-код
        var inviteCodeOption = await inviteCodeRepository.GetByCodeAsync(request.InviteCode, cancellationToken);
        
        if (inviteCodeOption.IsNone)
        {
            return new InvalidInviteCodeForJoinException(userId, request.InviteCode);
        }

        var inviteCode = inviteCodeOption.IfNone(() => throw new InvalidOperationException());

        if (!inviteCode.IsValid())
        {
            return new InvalidInviteCodeForJoinException(userId, request.InviteCode);
        }

        // Перевіряємо чи користувач вже є членом цієї організації
        var existingMemberships = await userOrganizationRepository.GetByUserIdAsync(userId, cancellationToken);
        
        if (existingMemberships.Any(m => m.OrganizationId == inviteCode.OrganizationId))
        {
            return new UserAlreadyInOrganizationException(userId, inviteCode.OrganizationId.Value);
        }

        try
        {
            // Використовуємо інвайт-код
            inviteCode.Use();
            inviteCodeRepository.Update(inviteCode);

            // Створюємо зв'язок з організацією
            var userOrganization = UserOrganization.New(
                userId,
                inviteCode.OrganizationId,
                inviteCode.AssignedRole,
                inviteCode.OrganizationalUnitId);

            userOrganizationRepository.Add(userOrganization);

            // Оновлюємо останню активну організацію
            user.SetLastActiveOrganization(inviteCode.OrganizationId);
            await userManager.UpdateAsync(user);

            await dbContext.SaveChangesAsync(cancellationToken);

            return userOrganization;
        }
        catch (Exception ex)
        {
            return new UnhandledUserException(userId, ex);
        }
    }
}

public class InvalidInviteCodeForJoinException(Guid userId, string code)
    : UserException(userId, $"Invalid or expired invite code: {code}");
