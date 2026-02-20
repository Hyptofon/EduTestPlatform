using FluentValidation;

namespace Application.Users.Commands;

public class SwitchOrganizationCommandValidator : AbstractValidator<SwitchOrganizationCommand>
{
    public SwitchOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required.");
    }
}
