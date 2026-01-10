using Application.Authentication.Exceptions;
using Application.Authentication.Models;
using Application.Common.Interfaces;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Either<AuthenticationException, AuthenticationResult>>
{
    public required string Token { get; init; }        // Expired Access Token
    public required string RefreshToken { get; init; } // Valid Refresh Token
}

public class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<RefreshTokenCommand, Either<AuthenticationException, AuthenticationResult>>
{
    public async Task<Either<AuthenticationException, AuthenticationResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Шукаємо Refresh Token в БД
            var storedRefreshToken = await context.RefreshTokens
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken);

            // 2. Валідація токена
            if (storedRefreshToken == null)
            {
                return new TokenRefreshFailedException("Refresh token does not exist");
            }

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return new TokenRefreshFailedException("Refresh token has expired");
            }

            if (storedRefreshToken.Invalidated)
            {
                return new TokenRefreshFailedException("Refresh token has been invalidated");
            }

            if (storedRefreshToken.Used)
            {
                return new TokenRefreshFailedException("Refresh token has been used");
            }

            // 3. Валідація JTI (необов'язково, але корисно для зв'язку Access-Refresh)
            // Тут ми пропускаємо складну перевірку підпису AccessToken, довіряємо парі з БД

            // 4. Оновлюємо старий токен як використаний
            storedRefreshToken.Use();
            context.RefreshTokens.Update(storedRefreshToken);
            await context.SaveChangesAsync(cancellationToken);

            // 5. Генеруємо нову пару
            var user = storedRefreshToken.User;
            var roles = await userManager.GetRolesAsync(user);

            var newAccessToken = jwtTokenGenerator.GenerateToken(user, roles);
            var newRefreshToken = jwtTokenGenerator.GenerateRefreshToken();

            // 6. Зберігаємо новий Refresh Token
            var newRefreshTokenEntity = Domain.Users.RefreshToken.Create(
                newRefreshToken,
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.AddDays(7),
                user.Id);

            context.RefreshTokens.Add(newRefreshTokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            // 7. Повертаємо результат
            return new AuthenticationResult
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
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