using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateOrganizationDtoValidator : AbstractValidator<UpdateOrganizationDto>
{
    public UpdateOrganizationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Organization name is required")
            .MaximumLength(255)
            .WithMessage("Organization name must not exceed 255 characters");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.HeroImageUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.HeroImageUrl));
            
        RuleFor(x => x.WelcomeText)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.WelcomeText));
    }
}