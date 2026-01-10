using FluentValidation;

namespace Application.Organizations.Commands.Create;

public class CreateOrganizationalUnitCommandValidator : AbstractValidator<CreateOrganizationalUnitCommand>
{
    public CreateOrganizationalUnitCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().IsEnumName(typeof(Domain.Organizations.OrganizationalUnitType), caseSensitive: false);
    }
}