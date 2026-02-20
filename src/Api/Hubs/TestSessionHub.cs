using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

/// <summary>
/// SignalR hub для моніторингу тестових сесій в реальному часі.
/// Викладачі можуть підключатися до групи тесту для отримання оновлень.
/// </summary>
[Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
public class TestSessionHub : Hub<ITestSessionHubClient>
{
    /// <summary>
    /// Приєднатися до моніторингу конкретного тесту.
    /// </summary>
    public async Task JoinTestMonitoring(Guid testId)
    {
        var groupName = GetTestGroupName(testId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        // Можна відправити початкову статистику при підключенні
        await Clients.Caller.ProgressUpdated(new ProgressUpdatedMessage
        {
            TestId = testId,
            ActiveSessions = 0,
            CompletedSessions = 0,
            TotalViolations = 0,
            AverageProgress = 0
        });
    }

    /// <summary>
    /// Відключитися від моніторингу тесту.
    /// </summary>
    public async Task LeaveTestMonitoring(Guid testId)
    {
        var groupName = GetTestGroupName(testId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Приєднатися до моніторингу конкретної сесії студента.
    /// </summary>
    public async Task JoinSessionMonitoring(Guid sessionId)
    {
        var groupName = GetSessionGroupName(sessionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Відключитися від моніторингу сесії.
    /// </summary>
    public async Task LeaveSessionMonitoring(Guid sessionId)
    {
        var groupName = GetSessionGroupName(sessionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Очищення при відключенні відбувається автоматично
        await base.OnDisconnectedAsync(exception);
    }

    public static string GetTestGroupName(Guid testId) => $"test_{testId}";
    public static string GetSessionGroupName(Guid sessionId) => $"session_{sessionId}";
}
