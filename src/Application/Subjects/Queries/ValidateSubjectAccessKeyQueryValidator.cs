using FluentValidation;

namespace Application.Subjects.Queries;

public class ValidateSubjectAccessKeyQueryValidator : AbstractValidator<ValidateSubjectAccessKeyQuery>
{
    public ValidateSubjectAccessKeyQueryValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithMessage("Subject ID is required.");
    }
}
