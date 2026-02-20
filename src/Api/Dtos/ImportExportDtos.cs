namespace Api.Dtos;

/// <summary>
/// Результат імпорту питань.
/// </summary>
public record ImportResultDto
{
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public List<string> Errors { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
}
