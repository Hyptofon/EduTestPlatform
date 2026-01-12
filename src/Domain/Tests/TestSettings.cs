namespace Domain.Tests;

public class TestSettings
{
    public int? TimeLimitMinutes { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? BankModeQuestionCount { get; private set; }
    public bool ShuffleAnswers { get; private set; }
    public ResultDisplayPolicy ResultDisplayPolicy { get; private set; }
    public int MaxAttempts { get; private set; }
    public bool IsPublic { get; private set; }

    private TestSettings(
        int? timeLimitMinutes,
        DateTime? startDate,
        DateTime? endDate,
        int? bankModeQuestionCount,
        bool shuffleAnswers,
        ResultDisplayPolicy resultDisplayPolicy,
        int maxAttempts,
        bool isPublic)
    {
        TimeLimitMinutes = timeLimitMinutes;
        StartDate = startDate;
        EndDate = endDate;
        BankModeQuestionCount = bankModeQuestionCount;
        ShuffleAnswers = shuffleAnswers;
        ResultDisplayPolicy = resultDisplayPolicy;
        MaxAttempts = maxAttempts;
        IsPublic = isPublic;
    }

    public static TestSettings Default()
    {
        return new TestSettings(
            null,
            null,
            null,
            null,
            false,
            ResultDisplayPolicy.Immediate,
            1,
            true);
    }

    public static TestSettings New(
        int? timeLimitMinutes,
        DateTime? startDate,
        DateTime? endDate,
        int? bankModeQuestionCount,
        bool shuffleAnswers,
        ResultDisplayPolicy resultDisplayPolicy,
        int maxAttempts,
        bool isPublic)
    {
        if (maxAttempts < 1)
            throw new ArgumentException("Max attempts must be at least 1", nameof(maxAttempts));

        if (bankModeQuestionCount.HasValue && bankModeQuestionCount.Value < 1)
            throw new ArgumentException("Bank mode question count must be at least 1", nameof(bankModeQuestionCount));

        return new TestSettings(
            timeLimitMinutes,
            startDate,
            endDate,
            bankModeQuestionCount,
            shuffleAnswers,
            resultDisplayPolicy,
            maxAttempts,
            isPublic);
    }
}