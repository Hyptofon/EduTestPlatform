using FluentValidation;

namespace Application.TestSessions.Commands;

public class CompleteTestSessionCommandValidator : AbstractValidator<CompleteTestSessionCommand>
{
    public CompleteTestSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}