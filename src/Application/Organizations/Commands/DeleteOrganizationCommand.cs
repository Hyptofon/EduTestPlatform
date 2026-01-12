using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Organizations.Exceptions;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.Organizations.Commands;

public record DeleteOrganizationCommand(Guid OrganizationId)
    : IRequest<Either<OrganizationException, Organization>>;

public class DeleteOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationalUnitRepository unitRepository,
    IUserOrganizationRepository userOrganizationRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteOrganizationCommand, Either<OrganizationException, Organization>>
{
    public async Task<Either<OrganizationException, Organization>> Handle(
        DeleteOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(request.OrganizationId);
        var existingOrganization = await organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        return await existingOrganization.MatchAsync(
            organization => DeleteEntity(organization, cancellationToken),
            () => Task.FromResult<Either<OrganizationException, Organization>>(
                new OrganizationNotFoundException(organizationId)));
    }

    private async Task<Either<OrganizationException, Organization>> DeleteEntity(
        Organization organization,
        CancellationToken cancellationToken)
    {
        var hasUnits = await unitRepository.GetByOrganizationIdAsync(organization.Id, cancellationToken);
        if (hasUnits.Any())
        {
            return new OrganizationCannotBeDeletedException(organization.Id);
        }

        var hasUsers = await userOrganizationRepository.GetByOrganizationIdAsync(organization.Id, cancellationToken);
        if (hasUsers.Any())
        {
            return new OrganizationCannotBeDeletedException(organization.Id);
        }

        try
        {
            organizationRepository.Delete(organization);
            await dbContext.SaveChangesAsync(cancellationToken);

            return organization;
        }
        catch (Exception exception)
        {
            return new UnhandledOrganizationException(organization.Id, exception);
        }
    }
}