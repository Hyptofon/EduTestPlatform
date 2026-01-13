using Domain.Tests;

namespace Api.Dtos;

public record TestSessionDto(
    Guid Id,
    Guid TestId,
    Guid StudentId,
    string Status,
    DateTime StartedAt,
    DateTime? FinishedAt,
    int TotalScore,
    int MaxScore,
    double Percentage,
    int ViolationCount,
    string? TeacherComment)
{
    public static TestSessionDto FromDomainModel(TestSession session)
        => new(
            session.Id.Value,
            session.TestId.Value,
            session.StudentId,
            session.Status.ToString(),
            session.StartedAt,
            session.FinishedAt,
            session.TotalScore,
            session.MaxScore,
            session.GetPercentage(),
            session.Violations.Count,
            session.TeacherComment);
}

public record StartTestSessionDto(Guid TestId);

public record SubmitAnswerDto(
    Guid QuestionId,
    List<Guid>? SelectedAnswerIds,
    string? TextAnswer);

public record RecordViolationDto(
    string ViolationType,
    Guid? QuestionId,
    int DurationSeconds);

public record GradeAnswerDto(
    Guid QuestionId,
    int Points);