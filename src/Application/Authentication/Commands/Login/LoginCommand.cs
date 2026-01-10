using Application.Authentication.Exceptions;
using Application.Authentication.Models;
using Application.Common.Interfaces;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Authentication.Commands.Login;

public record LoginCommand : IRequest<Either<AuthenticationException, AuthenticationResult>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IApplicationDbContext context)
    : IRequestHandler<LoginCommand, Either<AuthenticationException, AuthenticationResult>>
{
    public async Task<Either<AuthenticationException, AuthenticationResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Шукаємо користувача
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return new UserNotFoundException(request.Email);
        }

        // 2. Перевіряємо пароль
        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            return new InvalidCredentialsException();
        }

        try
        {
            var roles = await userManager.GetRolesAsync(user);
            
            var accessToken = jwtTokenGenerator.GenerateToken(user, roles); // перейменував для ясності
            var refreshTokenString = jwtTokenGenerator.GenerateRefreshToken(); // ПЕРЕЙМЕНУВАВ змінну

            // Тепер RefreshToken.Create точно звертається до класу
            var refreshTokenEntity = Domain.Users.RefreshToken.Create(
                refreshTokenString,
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.AddDays(7),
                user.Id);

            context.RefreshTokens.Add(refreshTokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            return new AuthenticationResult
            {
                Token = accessToken,
                RefreshToken = refreshTokenString,
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            };
        }
        catch (Exception ex)
        {
            return new UnhandledAuthenticationException(ex);
        }
    }
}