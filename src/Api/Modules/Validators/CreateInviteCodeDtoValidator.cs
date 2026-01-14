using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateInviteCodeDtoValidator : AbstractValidator<CreateInviteCodeDto>
{
    public CreateInviteCodeDtoValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required");
        
        RuleFor(x => x.Code)
            .MinimumLength(6)
            .WithMessage("Code must be at least 6 characters long")
            .MaximumLength(50)
            .WithMessage("Code must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid invite code type");

        RuleFor(x => x.AssignedRole)
            .NotEmpty()
            .WithMessage("Assigned role is required");

        RuleFor(x => x.MaxUses)
            .GreaterThan(0)
            .When(x => x.MaxUses.HasValue)
            .WithMessage("Max uses must be greater than 0");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("Expiration date must be in the future");
    }
}