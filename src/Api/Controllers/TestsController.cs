using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces;
using Application.Tests.Commands;
using Application.Tests.Queries;
using Domain.Tests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("tests")]
[Authorize]
public class TestsController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TestDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTestByIdQuery(id);
        var test = await sender.Send(query, cancellationToken);

        return test.Match<ActionResult<TestDto>>(
            t => TestDto.FromDomainModel(t),
            () => NotFound());
    }

    [HttpGet("subject/{subjectId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TestDto>>> GetBySubject(
        [FromRoute] Guid subjectId,
        CancellationToken cancellationToken)
    {
        var query = new GetTestsBySubjectQuery(subjectId);
        var tests = await sender.Send(query, cancellationToken);
        return tests.Select(TestDto.FromDomainModel).ToList();
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpGet("my-tests")]
    public async Task<ActionResult<IReadOnlyList<TestDto>>> GetMyTests(
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetTestsByTeacherQuery(currentUserService.UserId.Value);
        var tests = await sender.Send(query, cancellationToken);
        return tests.Select(TestDto.FromDomainModel).ToList();
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<TestDto>> Create(
        [FromBody] CreateTestDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var settings = TestSettings.New(
            request.Settings.TimeLimitMinutes,
            request.Settings.StartDate,
            request.Settings.EndDate,
            request.Settings.BankModeQuestionCount,
            request.Settings.ShuffleAnswers,
            Enum.Parse<ResultDisplayPolicy>(request.Settings.ResultDisplayPolicy),
            request.Settings.MaxAttempts,
            request.Settings.IsPublic);

        var command = new CreateTestCommand
        {
            SubjectId = request.SubjectId,
            CreatedByUserId = currentUserService.UserId.Value,
            Title = request.Title,
            Description = request.Description,
            Settings = settings,
            ContentJson = request.ContentJson
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestDto>>(
            t => TestDto.FromDomainModel(t),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TestDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTestDto request,
        CancellationToken cancellationToken)
    {
        var settings = TestSettings.New(
            request.Settings.TimeLimitMinutes,
            request.Settings.StartDate,
            request.Settings.EndDate,
            request.Settings.BankModeQuestionCount,
            request.Settings.ShuffleAnswers,
            Enum.Parse<ResultDisplayPolicy>(request.Settings.ResultDisplayPolicy),
            request.Settings.MaxAttempts,
            request.Settings.IsPublic);

        var command = new UpdateTestCommand
        {
            TestId = id,
            Title = request.Title,
            Description = request.Description,
            Settings = settings
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestDto>>(
            t => TestDto.FromDomainModel(t),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpPut("{id:guid}/content")]
    public async Task<ActionResult<TestDto>> UpdateContent(
        [FromRoute] Guid id,
        [FromBody] UpdateTestContentDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTestContentCommand
        {
            TestId = id,
            ContentJson = request.ContentJson
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestDto>>(
            t => TestDto.FromDomainModel(t),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<TestDto>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTestCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestDto>>(
            t => TestDto.FromDomainModel(t),
            e => e.ToObjectResult());
    }
}