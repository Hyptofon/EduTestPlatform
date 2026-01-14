using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateSubjectDtoValidator : AbstractValidator<UpdateSubjectDto>
{
    public UpdateSubjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Subject name is required")
            .MaximumLength(200)
            .WithMessage("Subject name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AccessType)
            .IsInEnum()
            .WithMessage("Invalid access type");
    }
}