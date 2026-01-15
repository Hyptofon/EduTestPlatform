namespace Application.Tests.Dtos;

// --- DTO для StartTestSessionCommand (простий формат для розрахунку балів) ---
public record TestContentDto
{
    public List<QuestionDto>? Questions { get; init; }
}

public record QuestionDto
{
    public Guid Id { get; init; }
    public int Points { get; init; }
}

// --- DTO для CompleteTestSessionCommand (повний формат для перевірки) ---
public record TestContentForGradingDto
{
    public List<QuestionForGradingDto>? Questions { get; init; }
}

public record QuestionForGradingDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public int Points { get; init; }
    public List<AnswerForGradingDto>? Answers { get; init; }
}

public record AnswerForGradingDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}