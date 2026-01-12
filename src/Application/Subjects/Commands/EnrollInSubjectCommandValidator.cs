using FluentValidation;

namespace Application.Subjects.Commands;

public class EnrollInSubjectCommandValidator : AbstractValidator<EnrollInSubjectCommand>
{
    public EnrollInSubjectCommandValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}