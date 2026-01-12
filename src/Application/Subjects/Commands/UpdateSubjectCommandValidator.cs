using Domain.Subjects;
using FluentValidation;

namespace Application.Subjects.Commands;

public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Subject name is required")
            .MaximumLength(255)
            .WithMessage("Subject name must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AccessType)
            .IsInEnum()
            .WithMessage("Invalid access type");

        RuleFor(x => x.AccessKey)
            .NotEmpty()
            .WithMessage("Access key is required for private subjects")
            .When(x => x.AccessType == SubjectAccessType.Private);
    }
}