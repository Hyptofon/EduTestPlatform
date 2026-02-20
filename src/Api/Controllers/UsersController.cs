using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces;
using Application.Users.Commands;
using Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Отримати профіль поточного користувача.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile(
        CancellationToken cancellationToken)
    {
        var query = new GetUserProfileQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.Match<ActionResult<UserProfileDto>>(
            profile => new UserProfileDto
            {
                Id = profile.Id,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                AvatarUrl = profile.AvatarUrl,
                LastActiveOrganizationId = profile.LastActiveOrganizationId,
                CreatedAt = profile.CreatedAt,
                Organizations = profile.Organizations.Select(o => new UserOrganizationInfoDto
                {
                    OrganizationId = o.OrganizationId,
                    OrganizationName = o.OrganizationName,
                    Role = o.Role,
                    OrganizationalUnitId = o.OrganizationalUnitId
                }).ToList()
            },
            error => error.ToObjectResult());
    }

    /// <summary>
    /// Оновити профіль поточного користувача.
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateCurrentUserProfile(
        [FromBody] UpdateUserProfileDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserProfileCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            AvatarUrl = request.AvatarUrl
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<UserProfileDto>>(
            user => new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                LastActiveOrganizationId = user.LastActiveOrganizationId?.Value,
                CreatedAt = user.CreatedAt,
                Organizations = []
            },
            error => error.ToObjectResult());
    }

    /// <summary>
    /// Отримати список організацій поточного користувача.
    /// </summary>
    [HttpGet("me/organizations")]
    public async Task<ActionResult<IReadOnlyList<UserOrganizationInfoDto>>> GetCurrentUserOrganizations(
        CancellationToken cancellationToken)
    {
        var query = new GetUserOrganizationsQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.Match<ActionResult<IReadOnlyList<UserOrganizationInfoDto>>>(
            orgs => orgs.Select(o => new UserOrganizationInfoDto
            {
                OrganizationId = o.OrganizationId,
                OrganizationName = o.OrganizationName,
                Role = o.Role,
                OrganizationalUnitId = o.OrganizationalUnitId
            }).ToList(),
            error => error.ToObjectResult());
    }

    /// <summary>
    /// Перемкнути активну організацію (workspace).
    /// </summary>
    [HttpPost("me/organizations/switch")]
    public async Task<ActionResult> SwitchOrganization(
        [FromBody] SwitchOrganizationDto request,
        CancellationToken cancellationToken)
    {
        var command = new SwitchOrganizationCommand
        {
            OrganizationId = request.OrganizationId
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult>(
            _ => Ok(new { message = "Organization switched successfully" }),
            error => error.ToObjectResult());
    }

    /// <summary>
    /// Приєднатися до нової організації через інвайт-код.
    /// </summary>
    [HttpPost("me/organizations/join")]
    public async Task<ActionResult<UserOrganizationInfoDto>> JoinOrganization(
        [FromBody] JoinOrganizationDto request,
        CancellationToken cancellationToken)
    {
        var command = new JoinOrganizationCommand
        {
            InviteCode = request.InviteCode
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<UserOrganizationInfoDto>>(
            uo => new UserOrganizationInfoDto
            {
                OrganizationId = uo.OrganizationId.Value,
                OrganizationName = uo.Organization?.Name ?? "Unknown",
                Role = uo.Role,
                OrganizationalUnitId = uo.OrganizationalUnitId?.Value
            },
            error => error.ToObjectResult());
    }
}
