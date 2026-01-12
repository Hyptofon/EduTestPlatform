using FluentValidation;

namespace Application.TestSessions.Commands;

public class RecordViolationCommandValidator : AbstractValidator<RecordViolationCommand>
{
    public RecordViolationCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.ViolationType).NotEmpty();
    }
}