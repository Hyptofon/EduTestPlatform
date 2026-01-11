namespace Api.Dtos;

public record StartTestSessionRequest(Guid TestId);

public record SubmitAnswerRequest(
    Guid QuestionId,
    List<Guid>? SelectedOptionIds,
    string? TextAnswer);