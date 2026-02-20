using FluentValidation;

namespace Application.Invites.Queries;

public class GenerateMagicLinkQueryValidator : AbstractValidator<GenerateMagicLinkQuery>
{
    public GenerateMagicLinkQueryValidator()
    {
        RuleFor(x => x.InviteCodeId)
            .NotEmpty()
            .WithMessage("Invite code ID is required.");

        RuleFor(x => x.BaseUrl)
            .NotEmpty()
            .WithMessage("Base URL is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Base URL must be a valid absolute URL.");
    }
}
