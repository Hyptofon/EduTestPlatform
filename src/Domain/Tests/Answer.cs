namespace Domain.Tests;

public class Answer
{
    public Guid Id { get; private set; }
    public string Text { get; private set; }
    public bool IsCorrect { get; private set; }

    private Answer(Guid id, string text, bool isCorrect)
    {
        Id = id;
        Text = text;
        IsCorrect = isCorrect;
    }

    public static Answer New(string text, bool isCorrect)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Answer text cannot be empty", nameof(text));

        return new Answer(Guid.NewGuid(), text, isCorrect);
    }
}