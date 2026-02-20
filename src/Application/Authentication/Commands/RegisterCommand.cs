using Application.Authentication.Exceptions;
using Application.Authentication.Interfaces;
using Application.Authentication.Models;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Audit;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Authentication.Commands;

/// <summary>
/// Команда реєстрації нового користувача.
/// ВАЖЛИВО: Інвайт-код є обов'язковим! Без валідного коду реєстрація неможлива.
/// Це забезпечує принцип Self-Enrollment де студенти реєструються через інвайти.
/// </summary>
public record RegisterCommand : IRequest<Either<AuthenticationException, AuthenticationResult>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    
    /// <summary>
    /// Інвайт-код для реєстрації. Обов'язковий параметр!
    /// Може приходити з Magic Link URL або вводитись вручну.
    /// </summary>
    public required string InviteCode { get; init; }
}

public class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IInviteCodeRepository inviteCodeRepository,
    IUserOrganizationRepository userOrganizationRepository,
    IApplicationDbContext dbContext,
    IAuditService auditService)
    : IRequestHandler<RegisterCommand, Either<AuthenticationException, AuthenticationResult>>
{
    public async Task<Either<AuthenticationException, AuthenticationResult>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // --- ЕТАП 1: Перевірка унікальності email ---
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return new UserAlreadyExistsException(request.Email);
        }

        // --- ЕТАП 2: Валідація інвайт-коду (ОБОВ'ЯЗКОВО) ---
        if (string.IsNullOrWhiteSpace(request.InviteCode))
        {
            return new InviteCodeRequiredException();
        }

        var inviteCodeOption = await inviteCodeRepository.GetByCodeAsync(request.InviteCode, cancellationToken);

        if (inviteCodeOption.IsNone)
        {
            return new InvalidInviteCodeException(request.InviteCode);
        }

        var inviteCode = inviteCodeOption.IfNone(() => throw new InvalidOperationException());

        if (!inviteCode.IsValid())
        {
            return new InvalidInviteCodeException(request.InviteCode);
        }

        try
        {
            // --- ЕТАП 3: Створення користувача ---
            var user = ApplicationUser.Create(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Email.Split('@')[0]);

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new UserCreationException(errors);
            }

            // --- ЕТАП 4: Використання інвайт-коду та прив'язка до організації ---
            inviteCode.Use();
            inviteCodeRepository.Update(inviteCode);

            var userOrganization = UserOrganization.New(
                user.Id,
                inviteCode.OrganizationId,
                inviteCode.AssignedRole,
                inviteCode.OrganizationalUnitId);

            userOrganizationRepository.Add(userOrganization);

            // --- ЕТАП 5: Призначення ролі ---
            await userManager.AddToRoleAsync(user, inviteCode.AssignedRole);

            await dbContext.SaveChangesAsync(cancellationToken);

            // --- ЕТАП 6: Генерація токенів ---
            var roles = await userManager.GetRolesAsync(user);
            var token = jwtTokenGenerator.GenerateToken(user, roles.ToList());
            var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
            user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await userManager.UpdateAsync(user);

            // --- ЕТАП 7: Логування реєстрації ---
            await auditService.LogAuthenticationAsync(
                AuditActions.UserRegister,
                user.Id,
                user.Email!,
                inviteCode.OrganizationId.Value,
                cancellationToken);

            return new AuthenticationResult
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                OrganizationId = inviteCode.OrganizationId.Value
            };
        }
        catch (Exception exception)
        {
            return new UnhandledAuthenticationException(exception);
        }
    }
}