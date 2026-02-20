using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces;
using Application.Subjects.Commands;
using Application.Subjects.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("subjects")]
[Authorize]
public class SubjectsController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSubjectByIdQuery(id);
        var subject = await sender.Send(query, cancellationToken);

        return subject.Match<ActionResult<SubjectDto>>(
            s => SubjectDto.FromDomainModel(s),
            () => NotFound());
    }

    [HttpGet("unit/{unitId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SubjectDto>>> GetByUnit(
        [FromRoute] Guid unitId,
        CancellationToken cancellationToken)
    {
        var query = new GetSubjectsByUnitQuery(unitId);
        var subjects = await sender.Send(query, cancellationToken);
        return subjects.Select(SubjectDto.FromDomainModel).ToList();
    }

    [HttpGet("my-enrolled")]
    public async Task<ActionResult<IReadOnlyList<SubjectEnrollmentDto>>> GetMyEnrolled(
        CancellationToken cancellationToken)
    {
        var query = new GetMyEnrolledSubjectsQuery();
        var enrollments = await sender.Send(query, cancellationToken);
        return enrollments.Select(SubjectEnrollmentDto.FromDomainModel).ToList();
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<SubjectDto>> Create(
        [FromBody] CreateSubjectDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new CreateSubjectCommand
        {
            OrganizationalUnitId = request.OrganizationalUnitId,
            CreatedByUserId = currentUserService.UserId.Value,
            Name = request.Name,
            Description = request.Description,
            AccessType = request.AccessType,
            AccessKey = request.AccessKey
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<SubjectDto>>(
            s => SubjectDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubjectDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSubjectDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSubjectCommand
        {
            SubjectId = id,
            Name = request.Name,
            Description = request.Description,
            AccessType = request.AccessType,
            AccessKey = request.AccessKey
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<SubjectDto>>(
            s => SubjectDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<SubjectDto>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSubjectCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<SubjectDto>>(
            s => SubjectDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }

    [HttpPost("{id:guid}/enroll")]
    public async Task<ActionResult<SubjectEnrollmentDto>> Enroll(
        [FromRoute] Guid id,
        [FromBody] EnrollInSubjectDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new EnrollInSubjectCommand
        {
            SubjectId = id,
            UserId = currentUserService.UserId.Value,
            AccessKey = request.AccessKey
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<SubjectEnrollmentDto>>(
            e => SubjectEnrollmentDto.FromDomainModel(e),
            error => error.ToObjectResult());
    }

    /// <summary>
    /// Перевіряє Access Key предмету БЕЗ enrollment.
    /// Використовується для UI: показати користувачу чи валідний ключ перед приєднанням.
    /// </summary>
    [HttpPost("{id:guid}/validate-access-key")]
    public async Task<ActionResult<SubjectAccessValidationDto>> ValidateAccessKey(
        [FromRoute] Guid id,
        [FromBody] ValidateAccessKeyDto request,
        CancellationToken cancellationToken)
    {
        var query = new ValidateSubjectAccessKeyQuery
        {
            SubjectId = id,
            AccessKey = request.AccessKey
        };

        var result = await sender.Send(query, cancellationToken);

        return result.Match<ActionResult<SubjectAccessValidationDto>>(
            r => new SubjectAccessValidationDto
            {
                IsValid = r.IsValid,
                IsPublic = r.IsPublic,
                SubjectName = r.SubjectName
            },
            error => error.ToObjectResult());
    }
}