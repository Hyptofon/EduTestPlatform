namespace Domain.Tests;

public class StudentAnswer
{
    public Guid QuestionId { get; private set; }
    public List<Guid> SelectedAnswerIds { get; private set; }
    public string? TextAnswer { get; private set; }
    public int PointsEarned { get; private set; }
    public DateTime AnsweredAt { get; private set; }

    private StudentAnswer(
        Guid questionId,
        List<Guid> selectedAnswerIds,
        string? textAnswer,
        int pointsEarned,
        DateTime answeredAt)
    {
        QuestionId = questionId;
        SelectedAnswerIds = selectedAnswerIds;
        TextAnswer = textAnswer;
        PointsEarned = pointsEarned;
        AnsweredAt = answeredAt;
    }

    public static StudentAnswer New(
        Guid questionId,
        List<Guid>? selectedAnswerIds = null,
        string? textAnswer = null)
    {
        return new StudentAnswer(
            questionId,
            selectedAnswerIds ?? new List<Guid>(),
            textAnswer,
            0,
            DateTime.UtcNow);
    }

    public void Grade(int points)
    {
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));

        PointsEarned = points;
    }
}