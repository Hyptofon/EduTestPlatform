using Application.Authentication.Exceptions;
using Application.Authentication.Models;
using Application.Common.Interfaces;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Commands.Register;

public record RegisterCommand : IRequest<Either<AuthenticationException, AuthenticationResult>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string InviteCode { get; init; } 
}

public class RegisterCommandHandler(
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<RegisterCommand, Either<AuthenticationException, AuthenticationResult>>
{
    public async Task<Either<AuthenticationException, AuthenticationResult>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Валідація Інвайт-коду 
        var orgUnit = await context.OrganizationalUnits
            .FirstOrDefaultAsync(x => x.InviteCode == request.InviteCode, cancellationToken);

        if (orgUnit == null)
        {
            return new InvalidInviteCodeException(request.InviteCode);
        }

        // 2. Перевірка на існування користувача
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new UserAlreadyExistsException(request.Email);
        }

        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new UserCreationException(errors);
            }
            
            var roleName = UserRole.Student.ToString(); 
            
             await userManager.AddToRoleAsync(user, roleName); 

            var userOrg = new UserOrganization
            {
                UserId = user.Id,
                OrganizationalUnitId = orgUnit.Id,
                Role = UserRole.Student
            };
            
            context.Users.Attach(user);
            user.Organizations.Add(userOrg);
            await context.SaveChangesAsync(cancellationToken);


            var roles = new List<string> { roleName };
            
            var accessToken = jwtTokenGenerator.GenerateToken(user, roles);
            var refreshTokenString = jwtTokenGenerator.GenerateRefreshToken(); // ПЕРЕЙМЕНУВАВ

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
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            };
        }
        catch (Exception exception)
        {
            return new UnhandledAuthenticationException(exception);
        }
    }
}