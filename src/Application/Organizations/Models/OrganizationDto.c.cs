namespace Application.Organizations.Models;

public record OrganizationDto(
    Guid Id,
    string Name,
    string Type,
    Guid? ParentId,
    string? InviteCode
);