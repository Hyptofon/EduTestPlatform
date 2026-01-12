using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.OrganizationalUnits.Exceptions;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.OrganizationalUnits.Commands;

public record UpdateOrganizationalUnitCommand : IRequest<Either<OrganizationalUnitException, OrganizationalUnit>>
{
    public required Guid UnitId { get; init; }
    public required string Name { get; init; }
    public string? Settings { get; init; }
}

public class UpdateOrganizationalUnitCommandHandler(
    IOrganizationalUnitRepository unitRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateOrganizationalUnitCommand, Either<OrganizationalUnitException, OrganizationalUnit>>
{
    public async Task<Either<OrganizationalUnitException, OrganizationalUnit>> Handle(
        UpdateOrganizationalUnitCommand request,
        CancellationToken cancellationToken)
    {
        var unitId = new OrganizationalUnitId(request.UnitId);
        var existingUnit = await unitRepository.GetByIdAsync(unitId, cancellationToken);

        return await existingUnit.MatchAsync(
            unit => UpdateEntity(unit, request, cancellationToken),
            () => Task.FromResult<Either<OrganizationalUnitException, OrganizationalUnit>>(
                new OrganizationalUnitNotFoundException(unitId)));
    }

    private async Task<Either<OrganizationalUnitException, OrganizationalUnit>> UpdateEntity(
        OrganizationalUnit unit,
        UpdateOrganizationalUnitCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            unit.UpdateDetails(request.Name, request.Settings);

            unitRepository.Update(unit);
            await dbContext.SaveChangesAsync(cancellationToken);

            return unit;
        }
        catch (Exception exception)
        {
            return new UnhandledOrganizationalUnitException(unit.Id, exception);
        }
    }
}