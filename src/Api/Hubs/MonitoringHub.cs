using System.Security.Claims;
using Application.Common.Interfaces;
using Domain.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Api.Hubs;

[Authorize]
public class MonitoringHub(IApplicationDbContext context) : Hub
{
    // Студент підключається до сесії тестування
    public async Task JoinSession(Guid sessionId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new HubException("Unauthorized");
        }

        // Перевірка, що сесія належить цьому студенту
        var session = await context.TestSessions
            .FirstOrDefaultAsync(x => x.Id == new Domain.TestSessions.TestSessionId(sessionId));

        if (session == null)
        {
            throw new HubException("Session not found");
        }

        if (session.StudentId != userGuid)
        {
            throw new HubException("Unauthorized access to session");
        }

        // Додаємо в групу сесії
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        
        // Повідомляємо викладача, що студент онлайн
        await Clients.Group($"teacher-session-{sessionId}")
            .SendAsync("StudentOnline", new
            {
                SessionId = sessionId,
                StudentId = userGuid,
                Timestamp = DateTime.UtcNow
            });
    }

    // Викладач підключається для моніторингу тесту
    public async Task JoinTeacherMonitoring(Guid testId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new HubException("Unauthorized");
        }

        // Перевірка, що тест належить цьому викладачу
        var test = await context.Tests
            .FirstOrDefaultAsync(x => x.Id == new Domain.Tests.TestId(testId));

        if (test == null)
        {
            throw new HubException("Test not found");
        }

        if (test.AuthorId != userGuid)
        {
            // Перевірка на роль Admin/Manager
            var isAdminOrManager = Context.User?.IsInRole("Admin") == true || 
                                   Context.User?.IsInRole("Manager") == true;
            
            if (!isAdminOrManager)
            {
                throw new HubException("Unauthorized access to test monitoring");
            }
        }

        // Додаємо в групу моніторингу
        await Groups.AddToGroupAsync(Context.ConnectionId, $"teacher-test-{testId}");

        // Відправляємо поточний стан всіх активних сесій
        var activeSessions = await context.TestSessions
            .Where(x => x.TestId == new Domain.Tests.TestId(testId) && 
                       x.Status == Domain.TestSessions.TestSessionStatus.InProgress)
            .Select(x => new
            {
                SessionId = x.Id.Value,
                StudentId = x.StudentId,
                StartedAt = x.StartedAt,
                AnsweredCount = x.Answers.Count,
                ViolationCount = x.Violations.Count
            })
            .ToListAsync();

        await Clients.Caller.SendAsync("InitialSessionsState", activeSessions);
    }

    // Студент надсилає відповідь
    public async Task SubmitAnswer(Guid sessionId, Guid questionId, object answer)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new HubException("Unauthorized");
        }

        // Оновлюємо прогрес у викладача в реальному часі
        await Clients.Group($"teacher-session-{sessionId}")
            .SendAsync("StudentAnswered", new
            {
                SessionId = sessionId,
                StudentId = userGuid,
                QuestionId = questionId,
                Timestamp = DateTime.UtcNow
            });
    }

    // Студент втратив фокус (відкрив іншу вкладку)
    public async Task ReportViolation(Guid sessionId, string violationType, string context)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new HubException("Unauthorized");
        }

        var session = await context.TestSessions
            .FirstOrDefaultAsync(x => x.Id == new Domain.TestSessions.TestSessionId(sessionId));

        if (session == null || session.StudentId != userGuid)
        {
            throw new HubException("Unauthorized");
        }

        // Реєструємо порушення
        session.RegisterViolation(violationType, context, DateTime.UtcNow);
        context.TestSessions.Update(session);
        await context.SaveChangesAsync(default);

        // Повідомляємо викладача
        await Clients.Group($"teacher-session-{sessionId}")
            .SendAsync("ViolationDetected", new
            {
                SessionId = sessionId,
                StudentId = userGuid,
                ViolationType = violationType,
                Context = context,
                Timestamp = DateTime.UtcNow,
                TotalViolations = session.Violations.Count
            });
    }

    // Heartbeat для визначення Online/Offline статусу
    public async Task SendHeartbeat(Guid sessionId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return;
        }

        // Оновлюємо timestamp останньої активності (можна зберігати в Redis)
        await Clients.Group($"teacher-session-{sessionId}")
            .SendAsync("StudentHeartbeat", new
            {
                SessionId = sessionId,
                StudentId = userGuid,
                Timestamp = DateTime.UtcNow
            });
    }

    // При завершенні тесту
    public async Task CompleteSession(Guid sessionId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new HubException("Unauthorized");
        }

        var session = await context.TestSessions
            .FirstOrDefaultAsync(x => x.Id == new Domain.TestSessions.TestSessionId(sessionId));

        if (session == null || session.StudentId != userGuid)
        {
            throw new HubException("Unauthorized");
        }

        session.Complete(DateTime.UtcNow);
        context.TestSessions.Update(session);
        await context.SaveChangesAsync(default);

        // Повідомляємо викладача
        await Clients.Group($"teacher-session-{sessionId}")
            .SendAsync("SessionCompleted", new
            {
                SessionId = sessionId,
                StudentId = userGuid,
                CompletedAt = DateTime.UtcNow,
                TotalAnswers = session.Answers.Count,
                TotalViolations = session.Violations.Count
            });

        // Виходимо з групи
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{sessionId}");
    }

    // Коли студент відключається
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            // Можна повідомити викладача про відключення
            // (знайти всі активні сесії цього студента)
            var activeSessions = await context.TestSessions
                .Where(x => x.StudentId == userGuid && 
                           x.Status == Domain.TestSessions.TestSessionStatus.InProgress)
                .Select(x => x.Id.Value)
                .ToListAsync();

            foreach (var sessionId in activeSessions)
            {
                await Clients.Group($"teacher-session-{sessionId}")
                    .SendAsync("StudentOffline", new
                    {
                        SessionId = sessionId,
                        StudentId = userGuid,
                        Timestamp = DateTime.UtcNow
                    });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}