using Api.Dtos;
using Application.Organizations.Commands.Create;
using Application.Organizations.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize] // Тільки авторизовані користувачі (або адміни)
public class OrganizationsController(ISender sender) : ApiController(sender)
{
    [HttpPost]
    [AllowAnonymous]
    // [Authorize(Roles = "SuperAdmin")] // У майбутньому розкоментувати для безпеки
    public async Task<IActionResult> Create([FromBody] CreateOrganizationDto dto)
    {
        var command = new CreateOrganizationalUnitCommand
        {
            Name = dto.Name,
            Type = dto.Type,
            ParentId = dto.ParentId,
            GenerateInvite = dto.GenerateInvite
        };

        var result = await Sender.Send(command);

        return result.Match(
            Right: id => Ok(new { Id = id }),
            Left: exception =>
            {
                // Проста обробка помилок (треба розширити switch в ApiController)
                return exception switch
                {
                    KeyNotFoundException => NotFound(new { error = exception.Message }),
                    ArgumentException => BadRequest(new { error = exception.Message }),
                    _ => StatusCode(500, new { error = exception.Message })
                };
            });
    }
    
    
    [HttpGet("{id}")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrganizationalUnitByIdQuery(id);
        var result = await Sender.Send(query);

        return result.Match(
            Right: dto => Ok(dto),
            Left: exception => exception switch
            {
                KeyNotFoundException => NotFound(new { error = exception.Message }),
                _ => StatusCode(500, new { error = exception.Message })
            });
    }
}