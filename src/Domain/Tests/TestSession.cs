namespace Domain.Tests;

public class TestSession
{
    public TestSessionId Id { get; }
    public TestId TestId { get; }
    public Guid StudentId { get; }
    public TestSessionStatus Status { get; private set; }
    public DateTime StartedAt { get; }
    public DateTime? FinishedAt { get; private set; }
    public List<StudentAnswer> Answers { get; private set; }
    public List<Violation> Violations { get; private set; }
    public int TotalScore { get; private set; }
    public int MaxScore { get; private set; }
    public string? TeacherComment { get; private set; }

    public Test? Test { get; private set; }

    private TestSession(
        TestSessionId id,
        TestId testId,
        Guid studentId,
        TestSessionStatus status,
        DateTime startedAt,
        DateTime? finishedAt,
        List<StudentAnswer> answers,
        List<Violation> violations,
        int totalScore,
        int maxScore,
        string? teacherComment)
    {
        Id = id;
        TestId = testId;
        StudentId = studentId;
        Status = status;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
        Answers = answers;
        Violations = violations;
        TotalScore = totalScore;
        MaxScore = maxScore;
        TeacherComment = teacherComment;
    }

    public static TestSession New(
        TestId testId,
        Guid studentId,
        int maxScore)
    {
        return new TestSession(
            TestSessionId.New(),
            testId,
            studentId,
            TestSessionStatus.InProgress,
            DateTime.UtcNow,
            null,
            new List<StudentAnswer>(),
            new List<Violation>(),
            0,
            maxScore,
            null);
    }
    
    public void SubmitAnswer(StudentAnswer answer)
    {
        if (Status != TestSessionStatus.InProgress)
            throw new InvalidOperationException("Cannot submit answer to non-active session");
        
        Answers.RemoveAll(a => a.QuestionId == answer.QuestionId);

        Answers.Add(answer);
    }

    public void RecordViolation(Violation violation)
    {
        if (Status != TestSessionStatus.InProgress)
            return;

        Violations.Add(violation);
    }

    public void Complete()
    {
        if (Status != TestSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress");

        Status = TestSessionStatus.Completed;
        FinishedAt = DateTime.UtcNow;
        TotalScore = Answers.Sum(a => a.PointsEarned);
    }

    public void Abandon()
    {
        if (Status != TestSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress");

        Status = TestSessionStatus.Abandoned;
        FinishedAt = DateTime.UtcNow;
    }

    public void GradeAnswer(Guid questionId, int points)
    {
        var answer = Answers.FirstOrDefault(a => a.QuestionId == questionId);
        if (answer == null)
            throw new InvalidOperationException("Answer not found");

        answer.Grade(points);
        TotalScore = Answers.Sum(a => a.PointsEarned);
    }

    public void AddTeacherComment(string comment)
    {
        TeacherComment = comment;
    }

    public double GetPercentage()
    {
        if (MaxScore == 0) return 0;
        return (double)TotalScore / MaxScore * 100;
    }
}