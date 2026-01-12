using FluentValidation;

namespace Application.TestSessions.Commands;

public class StartTestSessionCommandValidator : AbstractValidator<StartTestSessionCommand>
{
    public StartTestSessionCommandValidator()
    {
        RuleFor(x => x.TestId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
    }
}