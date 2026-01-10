using Domain.Common;
using Domain.Tests;
using Domain.Users;

namespace Domain.TestSessions;

public class TestSession : Entity<TestSessionId>
{
    public TestId TestId { get; private set; }
    public UserId StudentId { get; private set; }
    
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    
    public TestSessionStatus Status { get; private set; }
    
    // Зберігаємо як JSON
    public List<StudentAnswer> Answers { get; private set; } = new();
    
    // Зберігаємо як JSON (Anti-Cheat log)
    public List<SessionViolation> Violations { get; private set; } = new();
    
    public int TotalScore { get; private set; }

    private TestSession(TestSessionId id, TestId testId, UserId studentId, DateTime startedAt) : base(id)
    {
        TestId = testId;
        StudentId = studentId;
        StartedAt = startedAt;
        Status = TestSessionStatus.InProgress;
    }

    public static TestSession Start(TestId testId, UserId studentId, DateTime now)
    {
        return new TestSession(TestSessionId.New(), testId, studentId, now);
    }

    public void AddAnswer(Guid questionId, List<Guid> selectedOptions, string? textAnswer)
    {
        if (Status != TestSessionStatus.InProgress) 
            throw new InvalidOperationException("Session is closed.");

        // Видаляємо стару відповідь на це питання, якщо була (перезапис)
        Answers.RemoveAll(a => a.QuestionId == questionId);

        Answers.Add(new StudentAnswer
        {
            QuestionId = questionId,
            SelectedOptionIds = selectedOptions,
            TextAnswer = textAnswer
        });
    }

    public void RegisterViolation(string type, string context, DateTime now)
    {
        if (Status != TestSessionStatus.InProgress) return;

        Violations.Add(new SessionViolation
        {
            Timestamp = now,
            Type = type,
            Context = context
        });
    }

    public void Complete(DateTime now)
    {
        if (Status != TestSessionStatus.InProgress) return;
        
        Status = TestSessionStatus.Completed;
        FinishedAt = now;
    }

    public void SetScore(int score)
    {
        TotalScore = score;
        Status = TestSessionStatus.Evaluated;
    }
}