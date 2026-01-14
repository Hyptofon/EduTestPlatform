using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateTestDtoValidator : AbstractValidator<CreateTestDto>
{
    public CreateTestDtoValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithMessage("Subject ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Settings)
            .NotNull()
            .WithMessage("Test settings are required");

        RuleFor(x => x.Settings.TimeLimitMinutes)
            .GreaterThan(0)
            .When(x => x.Settings != null && x.Settings.TimeLimitMinutes.HasValue)
            .WithMessage("Time limit must be greater than 0");

        RuleFor(x => x.Settings.MaxAttempts)
            .GreaterThan(0)
            .When(x => x.Settings != null)
            .WithMessage("Max attempts must be greater than 0");

        RuleFor(x => x.ContentJson)
            .NotEmpty()
            .WithMessage("Test content is required")
            .Must(BeValidJson)
            .WithMessage("Content must be a valid JSON string");
    }

    private bool BeValidJson(string json)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}