using FluentValidation;

namespace Application.TestSessions.Commands.StartTestSession;

public class StartTestSessionCommandValidator : AbstractValidator<StartTestSessionCommand>
{
    public StartTestSessionCommandValidator()
    {
        RuleFor(x => x.TestId).NotEmpty();
    }
}