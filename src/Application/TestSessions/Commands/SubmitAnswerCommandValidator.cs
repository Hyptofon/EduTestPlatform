using FluentValidation;

namespace Application.TestSessions.Commands;

public class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
    }
}