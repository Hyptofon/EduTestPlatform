using Domain.Common;

namespace Domain.Tests;

public class TestSettings : ValueObject
{
    public TimeSpan TimeLimit { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool ShuffleQuestions { get; private set; }
    public int MaxAttempts { get; private set; }

    public TestSettings(TimeSpan timeLimit, DateTime? startDate, DateTime? endDate, bool shuffleQuestions, int maxAttempts)
    {
        TimeLimit = timeLimit;
        StartDate = startDate;
        EndDate = endDate;
        ShuffleQuestions = shuffleQuestions;
        MaxAttempts = maxAttempts;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TimeLimit;
        yield return ShuffleQuestions;
        yield return MaxAttempts;
        if (StartDate.HasValue) yield return StartDate;
        if (EndDate.HasValue) yield return EndDate;
    }
}