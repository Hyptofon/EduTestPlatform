namespace Api.Dtos;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string InviteCode);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string Token, string RefreshToken);