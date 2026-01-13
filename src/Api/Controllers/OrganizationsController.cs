using Api.Dtos;
using Api.Modules.Errors;
using Application.Organizations.Commands;
using Application.Organizations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("organizations")]
public class OrganizationsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrganizationDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var query = new GetAllOrganizationsQuery();
        var organizations = await sender.Send(query, cancellationToken);
        return organizations.Select(OrganizationDto.FromDomainModel).ToList();
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationByIdQuery(id);
        var organization = await sender.Send(query, cancellationToken);

        return organization.Match<ActionResult<OrganizationDto>>(
            o => OrganizationDto.FromDomainModel(o),
            () => NotFound());
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> Create(
        [FromBody] CreateOrganizationDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationCommand
        {
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            HeroImageUrl = request.HeroImageUrl,
            WelcomeText = request.WelcomeText
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<OrganizationDto>>(
            o => OrganizationDto.FromDomainModel(o),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "SuperAdmin,OrganizationAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrganizationDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateOrganizationDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationCommand
        {
            OrganizationId = id,
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            HeroImageUrl = request.HeroImageUrl,
            WelcomeText = request.WelcomeText
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<OrganizationDto>>(
            o => OrganizationDto.FromDomainModel(o),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<OrganizationDto>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteOrganizationCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<OrganizationDto>>(
            o => OrganizationDto.FromDomainModel(o),
            e => e.ToObjectResult());
    }
}