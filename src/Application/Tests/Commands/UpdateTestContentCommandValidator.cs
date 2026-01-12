using FluentValidation;

namespace Application.Tests.Commands;

public class UpdateTestContentCommandValidator : AbstractValidator<UpdateTestContentCommand>
{
    public UpdateTestContentCommandValidator()
    {
        RuleFor(x => x.TestId).NotEmpty();

        RuleFor(x => x.ContentJson)
            .NotEmpty()
            .WithMessage("Test content is required");
    }
}