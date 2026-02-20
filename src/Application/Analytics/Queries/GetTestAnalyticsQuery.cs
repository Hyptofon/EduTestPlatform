using Application.Common.Interfaces.Repositories;
using Domain.Tests;
using MediatR;

namespace Application.Analytics.Queries;

/// <summary>
/// Query для отримання статистики по групі/тесту.
/// </summary>
public record GetTestAnalyticsQuery : IRequest<TestAnalyticsResult>
{
    public required Guid TestId { get; init; }
}

public record TestAnalyticsResult
{
    public Guid TestId { get; init; }
    public string TestTitle { get; init; } = string.Empty;
    public int TotalSessions { get; init; }
    public int CompletedSessions { get; init; }
    public int InProgressSessions { get; init; }
    public int AbandonedSessions { get; init; }
    public decimal AverageScore { get; init; }
    public decimal MinScore { get; init; }
    public decimal MaxScore { get; init; }
    public decimal AverageCompletionTimeMinutes { get; init; }
    public int TotalViolations { get; init; }
    public IReadOnlyList<ScoreDistribution> ScoreDistribution { get; init; } = [];
    public IReadOnlyList<QuestionAnalytics> QuestionStats { get; init; } = [];
}

public record ScoreDistribution
{
    public string Range { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record QuestionAnalytics
{
    public int QuestionNumber { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public decimal CorrectPercentage { get; init; }
    public decimal AveragePoints { get; init; }
    public int TimesAnswered { get; init; }
}

public class GetTestAnalyticsQueryHandler(
    ITestRepository testRepository,
    ITestSessionRepository sessionRepository)
    : IRequestHandler<GetTestAnalyticsQuery, TestAnalyticsResult>
{
    public async Task<TestAnalyticsResult> Handle(
        GetTestAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var testId = new TestId(request.TestId);
        var testOption = await testRepository.GetByIdAsync(testId, cancellationToken);

        if (testOption.IsNone)
        {
            return new TestAnalyticsResult { TestId = request.TestId };
        }

        var test = testOption.IfNone(() => throw new InvalidOperationException());
        var sessions = await sessionRepository.GetByTestIdAsync(testId, cancellationToken);

        var completedSessions = sessions.Where(s => s.Status == TestSessionStatus.Completed).ToList();
        var inProgressSessions = sessions.Count(s => s.Status == TestSessionStatus.InProgress);
        var abandonedSessions = sessions.Count(s => s.Status == TestSessionStatus.Abandoned);

        var scores = completedSessions
            .Select(s => (decimal)s.TotalScore)
            .ToList();

        var completionTimes = completedSessions
            .Where(s => s.FinishedAt.HasValue)
            .Select(s => (s.FinishedAt!.Value - s.StartedAt).TotalMinutes)
            .ToList();

        var totalViolations = sessions.Sum(s => s.Violations?.Count ?? 0);
        var maxScore = completedSessions.FirstOrDefault()?.MaxScore ?? 100;

        // Розподіл балів
        var distribution = CalculateScoreDistribution(scores, maxScore);

        return new TestAnalyticsResult
        {
            TestId = request.TestId,
            TestTitle = test.Title,
            TotalSessions = sessions.Count,
            CompletedSessions = completedSessions.Count,
            InProgressSessions = inProgressSessions,
            AbandonedSessions = abandonedSessions,
            AverageScore = scores.Count > 0 ? scores.Average() : 0,
            MinScore = scores.Count > 0 ? scores.Min() : 0,
            MaxScore = scores.Count > 0 ? scores.Max() : 0,
            AverageCompletionTimeMinutes = completionTimes.Count > 0 
                ? (decimal)completionTimes.Average() 
                : 0,
            TotalViolations = totalViolations,
            ScoreDistribution = distribution,
            QuestionStats = [] // Потребує додаткового аналізу відповідей
        };
    }

    private List<ScoreDistribution> CalculateScoreDistribution(List<decimal> scores, decimal maxScore)
    {
        if (scores.Count == 0 || maxScore == 0) return [];

        var ranges = new[]
        {
            ("0-20%", 0m, 0.2m),
            ("21-40%", 0.2m, 0.4m),
            ("41-60%", 0.4m, 0.6m),
            ("61-80%", 0.6m, 0.8m),
            ("81-100%", 0.8m, 1.01m)
        };

        return ranges.Select(r =>
        {
            var count = scores.Count(s => 
                s / maxScore >= r.Item2 && 
                s / maxScore < r.Item3);

            return new ScoreDistribution
            {
                Range = r.Item1,
                Count = count,
                Percentage = scores.Count > 0 
                    ? Math.Round((decimal)count / scores.Count * 100, 1) 
                    : 0
            };
        }).ToList();
    }
}
