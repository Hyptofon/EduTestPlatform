using FluentValidation;

namespace Application.Organizations.Commands;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Organization name is required")
            .MaximumLength(255)
            .WithMessage("Organization name must not exceed 255 characters");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .WithMessage("Logo URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.HeroImageUrl)
            .MaximumLength(500)
            .WithMessage("Hero image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.HeroImageUrl));

        RuleFor(x => x.WelcomeText)
            .MaximumLength(1000)
            .WithMessage("Welcome text must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.WelcomeText));
    }
}