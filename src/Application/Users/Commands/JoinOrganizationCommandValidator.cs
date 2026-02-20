using FluentValidation;

namespace Application.Users.Commands;

public class JoinOrganizationCommandValidator : AbstractValidator<JoinOrganizationCommand>
{
    public JoinOrganizationCommandValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty()
            .WithMessage("Invite code is required.")
            .MaximumLength(50);
    }
}
