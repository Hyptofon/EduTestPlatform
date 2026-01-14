using FluentValidation;

namespace Application.Invites.Commands;

public class CreateInviteCodeCommandValidator : AbstractValidator<CreateInviteCodeCommand>
{
    public CreateInviteCodeCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        
        RuleFor(x => x.Code)
            .MaximumLength(100)
            .WithMessage("Invite code must not exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("Invite code can only contain letters, numbers, hyphens and underscores")
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid invite code type");

        RuleFor(x => x.AssignedRole)
            .NotEmpty()
            .WithMessage("Assigned role is required")
            .MaximumLength(50)
            .WithMessage("Assigned role must not exceed 50 characters");

        RuleFor(x => x.MaxUses)
            .GreaterThan(0)
            .WithMessage("Max uses must be greater than 0")
            .When(x => x.MaxUses.HasValue);
    }
}