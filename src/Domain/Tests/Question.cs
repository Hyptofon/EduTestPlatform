namespace Domain.Tests;

public class Question
{
    public Guid Id { get; private set; }
    public string Text { get; private set; }
    public QuestionType Type { get; private set; }
    public int Points { get; private set; }
    public List<Answer> Answers { get; private set; }

    private Question(
        Guid id,
        string text,
        QuestionType type,
        int points,
        List<Answer> answers)
    {
        Id = id;
        Text = text;
        Type = type;
        Points = points;
        Answers = answers;
    }

    public static Question New(
        string text,
        QuestionType type,
        int points,
        List<Answer> answers)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Question text cannot be empty", nameof(text));

        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));

        if (type == QuestionType.SingleChoice || type == QuestionType.MultipleChoice)
        {
            if (answers == null || answers.Count < 2)
                throw new ArgumentException("Choice questions must have at least 2 answers", nameof(answers));

            if (!answers.Any(a => a.IsCorrect))
                throw new ArgumentException("At least one answer must be marked as correct", nameof(answers));
        }

        return new Question(
            Guid.NewGuid(),
            text,
            type,
            points,
            answers ?? new List<Answer>());
    }
}