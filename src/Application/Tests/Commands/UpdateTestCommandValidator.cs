using FluentValidation;

namespace Application.Tests.Commands;

public class UpdateTestCommandValidator : AbstractValidator<UpdateTestCommand>
{
    public UpdateTestCommandValidator()
    {
        RuleFor(x => x.TestId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Test title is required")
            .MaximumLength(500)
            .WithMessage("Test title must not exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Settings)
            .NotNull()
            .WithMessage("Test settings are required");
    }
}