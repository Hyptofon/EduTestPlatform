using FluentValidation;

namespace Application.Organizations.Commands.GenerateInviteCode;

public class GenerateInviteCodeCommandValidator : AbstractValidator<GenerateInviteCodeCommand>
{
    public GenerateInviteCodeCommandValidator()
    {
        RuleFor(x => x.OrganizationalUnitId).NotEmpty();
        RuleFor(x => x.TargetRole).IsInEnum();
        RuleFor(x => x.MaxUses).GreaterThan(0).WithMessage("MaxUses must be at least 1");
    }
}