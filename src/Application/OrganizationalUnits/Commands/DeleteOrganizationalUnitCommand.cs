using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.OrganizationalUnits.Exceptions;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.OrganizationalUnits.Commands;

public record DeleteOrganizationalUnitCommand(Guid UnitId)
    : IRequest<Either<OrganizationalUnitException, OrganizationalUnit>>;

public class DeleteOrganizationalUnitCommandHandler(
    IOrganizationalUnitRepository unitRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteOrganizationalUnitCommand, Either<OrganizationalUnitException, OrganizationalUnit>>
{
    public async Task<Either<OrganizationalUnitException, OrganizationalUnit>> Handle(
        DeleteOrganizationalUnitCommand request,
        CancellationToken cancellationToken)
    {
        var unitId = new OrganizationalUnitId(request.UnitId);
        var existingUnit = await unitRepository.GetByIdAsync(unitId, cancellationToken);

        return await existingUnit.MatchAsync(
            unit => DeleteEntity(unit, cancellationToken),
            () => Task.FromResult<Either<OrganizationalUnitException, OrganizationalUnit>>(
                new OrganizationalUnitNotFoundException(unitId)));
    }

    private async Task<Either<OrganizationalUnitException, OrganizationalUnit>> DeleteEntity(
        OrganizationalUnit unit,
        CancellationToken cancellationToken)
    {
        var children = await unitRepository.GetByParentIdAsync(unit.Id, cancellationToken);
        if (children.Any())
        {
            return new OrganizationalUnitCannotBeDeletedException(unit.Id);
        }

        try
        {
            unitRepository.Delete(unit);
            await dbContext.SaveChangesAsync(cancellationToken);

            return unit;
        }
        catch (Exception exception)
        {
            return new UnhandledOrganizationalUnitException(unit.Id, exception);
        }
    }
}