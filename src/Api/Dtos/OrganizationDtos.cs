using Domain.Organizations;

namespace Api.Dtos;

public record OrganizationDto(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? HeroImageUrl,
    string? WelcomeText,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static OrganizationDto FromDomainModel(Organization organization)
        => new(
            organization.Id.Value,
            organization.Name,
            organization.LogoUrl,
            organization.HeroImageUrl,
            organization.WelcomeText,
            organization.CreatedAt,
            organization.UpdatedAt);
}

public record CreateOrganizationDto(
    string Name,
    string? LogoUrl,
    string? HeroImageUrl,
    string? WelcomeText);

public record UpdateOrganizationDto(
    string Name,
    string? LogoUrl,
    string? HeroImageUrl,
    string? WelcomeText);