using Api.Dtos;
using Api.Modules.Errors;
using Application.Invites.Commands;
using Application.Invites.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("invite-codes")]
[Authorize]
public class InviteCodesController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InviteCodeDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetInviteCodeByIdQuery(id);
        var inviteCode = await sender.Send(query, cancellationToken);

        return inviteCode.Match<ActionResult<InviteCodeDto>>(
            i => InviteCodeDto.FromDomainModel(i),
            () => NotFound());
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<ActionResult<IReadOnlyList<InviteCodeDto>>> GetByOrganization(
        [FromRoute] Guid organizationId,
        [FromQuery] bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInviteCodesByOrganizationQuery(organizationId, onlyActive);
        var inviteCodes = await sender.Send(query, cancellationToken);
        return inviteCodes.Select(InviteCodeDto.FromDomainModel).ToList();
    }

    [Authorize(Roles = "SuperAdmin,OrganizationAdmin")]
    [HttpPost]
    public async Task<ActionResult<InviteCodeDto>> Create(
        [FromBody] CreateInviteCodeDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateInviteCodeCommand
        {
            OrganizationId = request.OrganizationId,
            OrganizationalUnitId = request.OrganizationalUnitId,
            Code = request.Code,
            Type = request.Type,
            AssignedRole = request.AssignedRole,
            ExpiresAt = request.ExpiresAt,
            MaxUses = request.MaxUses
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<InviteCodeDto>>(
            i => InviteCodeDto.FromDomainModel(i),
            e => e.ToObjectResult());
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<ActionResult<InviteCodeDto>> Validate(
        [FromBody] ValidateInviteCodeDto request,
        CancellationToken cancellationToken)
    {
        var command = new ValidateAndUseInviteCodeCommand(request.Code);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<InviteCodeDto>>(
            i => InviteCodeDto.FromDomainModel(i),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "SuperAdmin,OrganizationAdmin")]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<InviteCodeDto>> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateInviteCodeCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<InviteCodeDto>>(
            i => InviteCodeDto.FromDomainModel(i),
            e => e.ToObjectResult());
    }
}