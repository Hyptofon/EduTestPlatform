using FluentValidation;

namespace Application.Enrollments.Commands.EnrollInSubject;

public class EnrollInSubjectCommandValidator : AbstractValidator<EnrollInSubjectCommand>
{
    public EnrollInSubjectCommandValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}