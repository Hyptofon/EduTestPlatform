using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Organizations.Exceptions;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.Organizations.Commands;

public record UpdateOrganizationCommand : IRequest<Either<OrganizationException, Organization>>
{
    public required Guid OrganizationId { get; init; }
    public required string Name { get; init; }
    public string? LogoUrl { get; init; }
    public string? HeroImageUrl { get; init; }
    public string? WelcomeText { get; init; }
}

public class UpdateOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateOrganizationCommand, Either<OrganizationException, Organization>>
{
    public async Task<Either<OrganizationException, Organization>> Handle(
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(request.OrganizationId);
        var existingOrganization = await organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        return await existingOrganization.MatchAsync(
            organization => UpdateEntity(organization, request, cancellationToken),
            () => Task.FromResult<Either<OrganizationException, Organization>>(
                new OrganizationNotFoundException(organizationId)));
    }

    private async Task<Either<OrganizationException, Organization>> UpdateEntity(
        Organization organization,
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            organization.UpdateName(request.Name);
            organization.UpdateBranding(request.LogoUrl, request.HeroImageUrl, request.WelcomeText);

            organizationRepository.Update(organization);
            await dbContext.SaveChangesAsync(cancellationToken);

            return organization;
        }
        catch (Exception exception)
        {
            return new UnhandledOrganizationException(organization.Id, exception);
        }
    }
}