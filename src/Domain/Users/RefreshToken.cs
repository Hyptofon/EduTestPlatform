using Domain.Common;

namespace Domain.Users;

public class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; }
    public string JwtId { get; private set; } 
    public DateTime CreationDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool Used { get; private set; }
    public bool Invalidated { get; private set; }
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; }

    private RefreshToken(Guid id, string token, string jwtId, DateTime creationDate, DateTime expiryDate, Guid userId) : base(id)
    {
        Token = token;
        JwtId = jwtId;
        CreationDate = creationDate;
        ExpiryDate = expiryDate;
        UserId = userId;
        Used = false;
        Invalidated = false;
    }

    public static RefreshToken Create(string token, string jwtId, DateTime expiryDate, Guid userId)
    {
        return new RefreshToken(Guid.NewGuid(), token, jwtId, DateTime.UtcNow, expiryDate, userId);
    }

    public void Use() => Used = true;

    public void Invalidate() => Invalidated = true;
}