using Domain.Tests;

namespace Api.Dtos;

public record TestDto(
    Guid Id,
    Guid SubjectId,
    Guid CreatedByUserId,
    string Title,
    string? Description,
    TestSettingsDto Settings,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static TestDto FromDomainModel(Test test)
        => new(
            test.Id.Value,
            test.SubjectId.Value,
            test.CreatedByUserId,
            test.Title,
            test.Description,
            TestSettingsDto.FromDomainModel(test.Settings),
            test.CreatedAt,
            test.UpdatedAt);
}

public record TestSettingsDto(
    int? TimeLimitMinutes,
    DateTime? StartDate,
    DateTime? EndDate,
    int? BankModeQuestionCount,
    bool ShuffleAnswers,
    string ResultDisplayPolicy,
    int MaxAttempts,
    bool IsPublic)
{
    public static TestSettingsDto FromDomainModel(TestSettings settings)
        => new(
            settings.TimeLimitMinutes,
            settings.StartDate,
            settings.EndDate,
            settings.BankModeQuestionCount,
            settings.ShuffleAnswers,
            settings.ResultDisplayPolicy.ToString(),
            settings.MaxAttempts,
            settings.IsPublic);
}

public record CreateTestDto(
    Guid SubjectId,
    string Title,
    string? Description,
    TestSettingsDto Settings,
    string ContentJson);

public record UpdateTestDto(
    string Title,
    string? Description,
    TestSettingsDto Settings);

public record UpdateTestContentDto(string ContentJson);