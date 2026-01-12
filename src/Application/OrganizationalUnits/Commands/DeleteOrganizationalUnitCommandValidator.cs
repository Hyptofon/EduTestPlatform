using FluentValidation;

namespace Application.OrganizationalUnits.Commands;

public class DeleteOrganizationalUnitCommandValidator : AbstractValidator<DeleteOrganizationalUnitCommand>
{
    public DeleteOrganizationalUnitCommandValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
    }
}