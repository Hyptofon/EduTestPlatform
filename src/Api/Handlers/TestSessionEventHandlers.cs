using Api.Hubs;
using Application.TestSessions.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Api.Handlers;

/// <summary>
/// Обробник подій для відправки SignalR сповіщень.
/// </summary>
public class TestSessionEventHandlers(
    IHubContext<TestSessionHub, ITestSessionHubClient> hubContext)
    : INotificationHandler<TestSessionStartedEvent>,
      INotificationHandler<AnswerSubmittedEvent>,
      INotificationHandler<ViolationRecordedEvent>,
      INotificationHandler<TestSessionCompletedEvent>
{
    public async Task Handle(TestSessionStartedEvent notification, CancellationToken cancellationToken)
    {
        var groupName = TestSessionHub.GetTestGroupName(notification.TestId);
        await hubContext.Clients.Group(groupName).SessionStarted(new SessionStartedMessage
        {
            SessionId = notification.SessionId,
            TestId = notification.TestId,
            UserId = notification.UserId,
            StudentName = "Student", // В реальному коді отримати з UserManager
            StartedAt = notification.StartedAt
        });
    }

    public async Task Handle(AnswerSubmittedEvent notification, CancellationToken cancellationToken)
    {
        var testGroup = TestSessionHub.GetTestGroupName(notification.TestId);
        var sessionGroup = TestSessionHub.GetSessionGroupName(notification.SessionId);

        var message = new AnswerSubmittedMessage
        {
            SessionId = notification.SessionId,
            UserId = notification.UserId,
            QuestionId = notification.QuestionId,
            QuestionNumber = notification.QuestionNumber,
            TotalQuestions = notification.TotalQuestions,
            AnsweredAt = notification.AnsweredAt
        };

        await Task.WhenAll(
            hubContext.Clients.Group(testGroup).AnswerSubmitted(message),
            hubContext.Clients.Group(sessionGroup).AnswerSubmitted(message)
        );
    }

    public async Task Handle(ViolationRecordedEvent notification, CancellationToken cancellationToken)
    {
        var testGroup = TestSessionHub.GetTestGroupName(notification.TestId);
        var sessionGroup = TestSessionHub.GetSessionGroupName(notification.SessionId);

        var message = new ViolationRecordedMessage
        {
            SessionId = notification.SessionId,
            UserId = notification.UserId,
            StudentName = "Student",
            ViolationType = notification.ViolationType,
            TotalViolations = notification.TotalViolations,
            RecordedAt = notification.RecordedAt
        };

        await Task.WhenAll(
            hubContext.Clients.Group(testGroup).ViolationRecorded(message),
            hubContext.Clients.Group(sessionGroup).ViolationRecorded(message)
        );
    }

    public async Task Handle(TestSessionCompletedEvent notification, CancellationToken cancellationToken)
    {
        var groupName = TestSessionHub.GetTestGroupName(notification.TestId);
        await hubContext.Clients.Group(groupName).SessionCompleted(new SessionCompletedMessage
        {
            SessionId = notification.SessionId,
            UserId = notification.UserId,
            StudentName = "Student",
            Score = notification.Score,
            MaxScore = notification.MaxScore,
            ViolationCount = notification.ViolationCount,
            CompletedAt = notification.CompletedAt
        });
    }
}
