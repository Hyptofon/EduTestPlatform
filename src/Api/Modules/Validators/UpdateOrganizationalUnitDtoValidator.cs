using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateOrganizationalUnitDtoValidator : AbstractValidator<UpdateOrganizationalUnitDto>
{
    public UpdateOrganizationalUnitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Settings)
            .Must(BeValidJson)
            .When(x => !string.IsNullOrEmpty(x.Settings))
            .WithMessage("Settings must be a valid JSON string");
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