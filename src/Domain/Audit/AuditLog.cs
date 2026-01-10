using Domain.Common;

namespace Domain.Audit;

public class AuditLog : Entity<Guid>
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } // "Login", "CreateTest", "DeleteOrg"
    public string Resource { get; private set; } // "Test-123", "Org-KPI"
    public DateTime Timestamp { get; private set; }
    public string IpAddress { get; private set; }
    public string Details { get; private set; } // JSON з деталями змін

    public AuditLog(Guid? userId, string action, string resource, string ipAddress, string details) : base(Guid.NewGuid())
    {
        UserId = userId;
        Action = action;
        Resource = resource;
        Timestamp = DateTime.UtcNow;
        IpAddress = ipAddress;
        Details = details;
    }
}