namespace Domain.Tests;

// Це об'єкт, який буде повністю серіалізований в базу
public class TestContent
{
    public List<TestSection> Sections { get; set; } = new();
}

public class TestSection
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = new();
}

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Points { get; set; }
    public List<AnswerOption> Options { get; set; } = new();
}

public enum QuestionType
{
    SingleChoice = 0,
    MultipleChoice = 1,
    ShortAnswer = 2,
    OpenEssay = 3
}

public class AnswerOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; } // Це поле треба буде приховувати при відправці студенту
}