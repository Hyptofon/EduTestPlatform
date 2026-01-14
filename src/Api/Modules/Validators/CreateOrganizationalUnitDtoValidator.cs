using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateOrganizationalUnitDtoValidator : AbstractValidator<CreateOrganizationalUnitDto>
{
    public CreateOrganizationalUnitDtoValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid organizational unit type");
            
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