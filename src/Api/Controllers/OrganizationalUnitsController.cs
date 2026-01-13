using Api.Dtos;
using Api.Modules.Errors;
using Application.OrganizationalUnits.Commands;
using Application.OrganizationalUnits.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("organizational-units")]
[Authorize]
public class OrganizationalUnitsController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationalUnitDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationalUnitByIdQuery(id);
        var unit = await sender.Send(query, cancellationToken);

        return unit.Match<ActionResult<OrganizationalUnitDto>>(
            u => OrganizationalUnitDto.FromDomainModel(u),
            () => NotFound());
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<ActionResult<IReadOnlyList<OrganizationalUnitDto>>> GetByOrganization(
        [FromRoute] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetUnitsByOrganizationQuery(organizationId);
        var units = await sender.Send(query, cancellationToken);
        return units.Select(OrganizationalUnitDto.FromDomainModel).ToList();
    }

    [Authorize(Roles = "SuperAdmin,OrganizationAdmin")]
    [HttpPost]
    public async Task<ActionResult<OrganizationalUnitDto>> Create(
        [FromBody] CreateOrganizationalUnitDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationalUnitCommand
        {
            OrganizationId = request.OrganizationId,
            ParentId = request.ParentId,
            Type = request.Type,
            Name = request.Name,
            Settings = request.Settings
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<OrganizationalUnitDto>>(
            u => OrganizationalUnitDto.FromDomainModel(u),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "SuperAdmin,OrganizationAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrganizationalUnitDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateOrganizationalUnitDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationalUnitCommand
        {
            UnitId = id,
            Name = request.Name,
            Settings = request.Settings
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<OrganizationalUnitDto>>(
            u => OrganizationalUnitDto.FromDomainModel(u),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "SuperAdmin,OrganizationAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<OrganizationalUnitDto>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteOrganizationalUnitCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<OrganizationalUnitDto>>(
            u => OrganizationalUnitDto.FromDomainModel(u),
            e => e.ToObjectResult());
    }
}