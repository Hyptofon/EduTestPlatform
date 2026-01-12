using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.OrganizationalUnits.Exceptions;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.OrganizationalUnits.Commands;

public record CreateOrganizationalUnitCommand : IRequest<Either<OrganizationalUnitException, OrganizationalUnit>>
{
    public required Guid OrganizationId { get; init; }
    public Guid? ParentId { get; init; }
    public required OrganizationalUnitType Type { get; init; }
    public required string Name { get; init; }
    public string? Settings { get; init; }
}

public class CreateOrganizationalUnitCommandHandler(
    IOrganizationalUnitRepository unitRepository,
    IOrganizationRepository organizationRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateOrganizationalUnitCommand, Either<OrganizationalUnitException, OrganizationalUnit>>
{
    public async Task<Either<OrganizationalUnitException, OrganizationalUnit>> Handle(
        CreateOrganizationalUnitCommand request,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(request.OrganizationId);
        var organizationExists = await organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organizationExists.IsNone)
        {
            return new OrganizationNotFoundForUnitException(organizationId);
        }

        if (request.ParentId.HasValue)
        {
            var parentId = new OrganizationalUnitId(request.ParentId.Value);
            var parentExists = await unitRepository.GetByIdAsync(parentId, cancellationToken);

            if (parentExists.IsNone)
            {
                return new ParentUnitNotFoundException(parentId);
            }
        }

        return await CreateEntity(request, organizationId, cancellationToken);
    }

    private async Task<Either<OrganizationalUnitException, OrganizationalUnit>> CreateEntity(
        CreateOrganizationalUnitCommand request,
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var parentId = request.ParentId.HasValue 
                ? new OrganizationalUnitId(request.ParentId.Value) 
                : null;

            var unit = OrganizationalUnit.New(
                OrganizationalUnitId.New(),
                organizationId,
                parentId,
                request.Type,
                request.Name,
                request.Settings);

            unitRepository.Add(unit);
            await dbContext.SaveChangesAsync(cancellationToken);

            return unit;
        }
        catch (Exception exception)
        {
            return new UnhandledOrganizationalUnitException(OrganizationalUnitId.Empty(), exception);
        }
    }
}