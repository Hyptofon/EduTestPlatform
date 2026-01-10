using Domain.Users;

namespace Application.Authentication.Models;

public record AuthenticationResult
{
    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required List<string> Roles { get; init; }
}