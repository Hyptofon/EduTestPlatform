using FluentValidation;

namespace Application.TestSessions.Commands;

public class GradeAnswerCommandValidator : AbstractValidator<GradeAnswerCommand>
{
    public GradeAnswerCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.Points).GreaterThanOrEqualTo(0);
    }
}