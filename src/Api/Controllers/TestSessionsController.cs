using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces;
using Application.TestSessions.Commands;
using Application.TestSessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("test-sessions")]
[Authorize]
public class TestSessionsController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TestSessionDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTestSessionByIdQuery(id);
        var session = await sender.Send(query, cancellationToken);

        return session.Match<ActionResult<TestSessionDto>>(
            s => TestSessionDto.FromDomainModel(s),
            () => NotFound());
    }

    [HttpGet("test/{testId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TestSessionDto>>> GetByTest(
        [FromRoute] Guid testId,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionsByTestQuery(testId);
        var sessions = await sender.Send(query, cancellationToken);
        return sessions.Select(TestSessionDto.FromDomainModel).ToList();
    }

    [HttpGet("my-sessions")]
    public async Task<ActionResult<IReadOnlyList<TestSessionDto>>> GetMySessions(
        CancellationToken cancellationToken)
    {
        var query = new GetMyTestSessionsQuery();
        var sessions = await sender.Send(query, cancellationToken);
        return sessions.Select(TestSessionDto.FromDomainModel).ToList();
    }

    [HttpPost("start")]
    public async Task<ActionResult<TestSessionDto>> Start(
        [FromBody] StartTestSessionDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new StartTestSessionCommand
        {
            TestId = request.TestId,
            StudentId = currentUserService.UserId.Value
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestSessionDto>>(
            s => TestSessionDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }

    [HttpPost("{sessionId:guid}/submit-answer")]
    public async Task<ActionResult<TestSessionDto>> SubmitAnswer(
        [FromRoute] Guid sessionId,
        [FromBody] SubmitAnswerDto request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitAnswerCommand
        {
            SessionId = sessionId,
            QuestionId = request.QuestionId,
            SelectedAnswerIds = request.SelectedAnswerIds,
            TextAnswer = request.TextAnswer
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestSessionDto>>(
            s => TestSessionDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }

    [HttpPost("{sessionId:guid}/record-violation")]
    public async Task<ActionResult<TestSessionDto>> RecordViolation(
        [FromRoute] Guid sessionId,
        [FromBody] RecordViolationDto request,
        CancellationToken cancellationToken)
    {
        var command = new RecordViolationCommand
        {
            SessionId = sessionId,
            ViolationType = request.ViolationType,
            QuestionId = request.QuestionId,
            DurationSeconds = request.DurationSeconds
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestSessionDto>>(
            s => TestSessionDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }

    [HttpPost("{sessionId:guid}/complete")]
    public async Task<ActionResult<TestSessionDto>> Complete(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTestSessionCommand(sessionId);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestSessionDto>>(
            s => TestSessionDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }

    [Authorize(Roles = "Teacher,OrganizationAdmin,SuperAdmin")]
    [HttpPost("{sessionId:guid}/grade-answer")]
    public async Task<ActionResult<TestSessionDto>> GradeAnswer(
        [FromRoute] Guid sessionId,
        [FromBody] GradeAnswerDto request,
        CancellationToken cancellationToken)
    {
        var command = new GradeAnswerCommand
        {
            SessionId = sessionId,
            QuestionId = request.QuestionId,
            Points = request.Points
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<TestSessionDto>>(
            s => TestSessionDto.FromDomainModel(s),
            e => e.ToObjectResult());
    }
}