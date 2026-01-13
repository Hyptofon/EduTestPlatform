using Api.Dtos;
using Domain.Subjects;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateSubjectDtoValidator : AbstractValidator<CreateSubjectDto>
{
    public CreateSubjectDtoValidator()
    {
        RuleFor(x => x.OrganizationalUnitId).NotEmpty();

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