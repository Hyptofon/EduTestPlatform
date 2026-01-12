using FluentValidation;

namespace Application.Tests.Commands;

public class DeleteTestCommandValidator : AbstractValidator<DeleteTestCommand>
{
    public DeleteTestCommandValidator()
    {
        RuleFor(x => x.TestId).NotEmpty();
    }
}