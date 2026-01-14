using Domain.Invites;

namespace Api.Dtos;

public record InviteCodeDto(
    Guid Id,
    Guid OrganizationId,
    Guid? OrganizationalUnitId,
    string Code,
    string? InviteLink, 
    string Type,
    string AssignedRole,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    int? MaxUses,
    int CurrentUses)
{
    public static InviteCodeDto FromDomainModel(InviteCode inviteCode, string? baseUrl = null)
    {
        var link = !string.IsNullOrEmpty(baseUrl) 
            ? $"{baseUrl}/register?inviteCode={inviteCode.Code}" 
            : null;

        return new(
            inviteCode.Id.Value,
            inviteCode.OrganizationId.Value,
            inviteCode.OrganizationalUnitId?.Value,
            inviteCode.Code,
            link,
            inviteCode.Type.ToString(),
            inviteCode.AssignedRole,
            inviteCode.IsActive,
            inviteCode.CreatedAt,
            inviteCode.ExpiresAt,
            inviteCode.MaxUses,
            inviteCode.CurrentUses);
    }
}

public record CreateInviteCodeDto(
    Guid OrganizationId,
    Guid? OrganizationalUnitId,
    string? Code, 
    InviteCodeType Type,
    string AssignedRole,
    DateTime? ExpiresAt,
    int? MaxUses);

public record ValidateInviteCodeDto(string Code);