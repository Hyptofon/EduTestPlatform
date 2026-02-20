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

    /// <summary>
    /// Імпортувати питання з CSV/Excel файлу в існуючий тест.
    /// </summary>
    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpPost("{id:guid}/import")]
    public async Task<ActionResult<ImportResultDto>> ImportQuestions(
        [FromRoute] Guid id,
        [FromForm] IFormFile file,
        [FromQuery] bool replaceExisting = false,
        CancellationToken cancellationToken = default)
    {
        var command = new ImportTestQuestionsCommand
        {
            TestId = id,
            File = file,
            ReplaceExisting = replaceExisting
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ImportResultDto>>(
            r => new ImportResultDto
            {
                ImportedCount = r.ImportedCount,
                SkippedCount = r.SkippedCount,
                Errors = r.Errors.ToList(),
                Warnings = r.Warnings.ToList()
            },
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Експортувати тест у CSV або JSON формат.
    /// </summary>
    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> Export(
        [FromRoute] Guid id,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        var query = new ExportTestQuery
        {
            TestId = id,
            Format = format
        };

        var result = await sender.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            r => File(r.Content, r.ContentType, r.FileName),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Завантажити шаблон для імпорту питань.
    /// </summary>
    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpGet("import-template")]
    public async Task<IActionResult> GetImportTemplate(
        CancellationToken cancellationToken = default)
    {
        var query = new GetImportTemplateQuery();
        var result = await sender.Send(query, cancellationToken);

        return File(result.Content, result.ContentType, result.FileName);
    }
}