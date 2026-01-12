using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Organizations.Exceptions;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.Organizations.Commands;

public record CreateOrganizationCommand : IRequest<Either<OrganizationException, Organization>>
{
    public required string Name { get; init; }
    public string? LogoUrl { get; init; }
    public string? HeroImageUrl { get; init; }
    public string? WelcomeText { get; init; }
}

public class CreateOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateOrganizationCommand, Either<OrganizationException, Organization>>
{
    public async Task<Either<OrganizationException, Organization>> Handle(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var existingOrganization = await organizationRepository.GetByNameAsync(request.Name, cancellationToken);

        return await existingOrganization.MatchAsync(
            org => Task.FromResult<Either<OrganizationException, Organization>>(
                new OrganizationAlreadyExistsException(request.Name)),
            () => CreateEntity(request, cancellationToken));
    }

    private async Task<Either<OrganizationException, Organization>> CreateEntity(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var organization = Organization.New(
                OrganizationId.New(),
                request.Name,
                request.LogoUrl,
                request.HeroImageUrl,
                request.WelcomeText);

            organizationRepository.Add(organization);
            await dbContext.SaveChangesAsync(cancellationToken);

            return organization;
        }
        catch (Exception exception)
        {
            return new UnhandledOrganizationException(OrganizationId.Empty(), exception);
        }
    }
}