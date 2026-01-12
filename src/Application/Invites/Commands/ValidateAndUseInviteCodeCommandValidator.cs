using FluentValidation;

namespace Application.Invites.Commands;

public class ValidateAndUseInviteCodeCommandValidator : AbstractValidator<ValidateAndUseInviteCodeCommand>
{
    public ValidateAndUseInviteCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Invite code is required");
    }
}