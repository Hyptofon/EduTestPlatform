namespace Application.Common.Interfaces;

/// <summary>
/// Сервіс для централізованого логування аудіт-подій.
/// Логує ВСІ важливі дії в системі.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Логує дію користувача або системи.
    /// </summary>
    Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        Guid? organizationId = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Логує дію автентифікації (логін, реєстрація, тощо).
    /// </summary>
    Task LogAuthenticationAsync(
        string action,
        Guid? userId,
        string userEmail,
        Guid? organizationId = null,
        CancellationToken cancellationToken = default);
}
