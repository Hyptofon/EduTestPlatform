namespace Domain.Audit;

/// <summary>
/// Сутність для зберігання аудіт-логів системи.
/// Дозволяє відстежувати всі важливі дії користувачів.
/// </summary>
public class AuditLog
{
    public AuditLogId Id { get; }
    public DateTime Timestamp { get; }
    public Guid? UserId { get; }
    public string UserEmail { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public Guid? OrganizationId { get; private set; }

    private AuditLog(
        AuditLogId id,
        DateTime timestamp,
        Guid? userId,
        string userEmail,
        string action,
        string entityType,
        string? entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress,
        string? userAgent,
        Guid? organizationId)
    {
        Id = id;
        Timestamp = timestamp;
        UserId = userId;
        UserEmail = userEmail;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OrganizationId = organizationId;
    }

    public static AuditLog Create(
        Guid? userId,
        string userEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        Guid? organizationId = null)
    {
        return new AuditLog(
            AuditLogId.New(),
            DateTime.UtcNow,
            userId,
            userEmail,
            action,
            entityType,
            entityId,
            oldValues,
            newValues,
            ipAddress,
            userAgent,
            organizationId);
    }
}

public readonly record struct AuditLogId(Guid Value)
{
    public static AuditLogId New() => new(Guid.NewGuid());
    public static AuditLogId Empty() => new(Guid.Empty);
}

/// <summary>
/// Типи дій для аудіту.
/// </summary>
public static class AuditActions
{
    // Автентифікація
    public const string UserLogin = "USER_LOGIN";
    public const string UserLogout = "USER_LOGOUT";
    public const string UserRegister = "USER_REGISTER";
    public const string PasswordChanged = "PASSWORD_CHANGED";
    
    // Тести
    public const string TestCreated = "TEST_CREATED";
    public const string TestUpdated = "TEST_UPDATED";
    public const string TestDeleted = "TEST_DELETED";
    public const string TestImported = "TEST_IMPORTED";
    public const string TestExported = "TEST_EXPORTED";
    
    // Сесії
    public const string SessionStarted = "SESSION_STARTED";
    public const string SessionCompleted = "SESSION_COMPLETED";
    public const string SessionAbandoned = "SESSION_ABANDONED";
    public const string ViolationRecorded = "VIOLATION_RECORDED";
    
    // Організації
    public const string OrganizationCreated = "ORGANIZATION_CREATED";
    public const string OrganizationUpdated = "ORGANIZATION_UPDATED";
    public const string UserJoinedOrganization = "USER_JOINED_ORGANIZATION";
    public const string UserLeftOrganization = "USER_LEFT_ORGANIZATION";
    
    // Інвайти
    public const string InviteCodeCreated = "INVITE_CODE_CREATED";
    public const string InviteCodeUsed = "INVITE_CODE_USED";
}
