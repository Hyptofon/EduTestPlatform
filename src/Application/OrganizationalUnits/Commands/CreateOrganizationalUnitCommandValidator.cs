using FluentValidation;

namespace Application.OrganizationalUnits.Commands;

public class CreateOrganizationalUnitCommandValidator : AbstractValidator<CreateOrganizationalUnitCommand>
{
    public CreateOrganizationalUnitCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Unit name is required")
            .MaximumLength(255)
            .WithMessage("Unit name must not exceed 255 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid unit type");

        RuleFor(x => x.Settings)
            .MaximumLength(2000)
            .WithMessage("Settings must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Settings));
    }
}