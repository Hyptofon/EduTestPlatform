using Domain.Organizations;

namespace Api.Dtos;

public record OrganizationalUnitDto(
    Guid Id,
    Guid OrganizationId,
    Guid? ParentId,
    string Type,
    string Name,
    string? Settings,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static OrganizationalUnitDto FromDomainModel(OrganizationalUnit unit)
        => new(
            unit.Id.Value,
            unit.OrganizationId.Value,
            unit.ParentId?.Value,
            unit.Type.ToString(),
            unit.Name,
            unit.Settings,
            unit.CreatedAt,
            unit.UpdatedAt);
}

public record CreateOrganizationalUnitDto(
    Guid OrganizationId,
    Guid? ParentId,
    OrganizationalUnitType Type,
    string Name,
    string? Settings);

public record UpdateOrganizationalUnitDto(
    string Name,
    string? Settings);