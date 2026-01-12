using FluentValidation;

namespace Application.Invites.Commands;

public class DeactivateInviteCodeCommandValidator : AbstractValidator<DeactivateInviteCodeCommand>
{
    public DeactivateInviteCodeCommandValidator()
    {
        RuleFor(x => x.InviteCodeId).NotEmpty();
    }
}