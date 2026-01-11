using Api.Dtos;
using Application.TestSessions.Commands.CompleteTestSession;
using Application.TestSessions.Commands.StartTestSession;
using Application.TestSessions.Commands.SubmitAnswer;
using Application.TestSessions.Queries.GetActiveTestSessions;
using Application.TestSessions.Queries.GetMyTestSession;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class TestSessionsController(ISender sender) : ApiController(sender)
{
    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartTestSessionRequest request)
    {
        var command = new StartTestSessionCommand
        {
            TestId = request.TestId
        };

        var result = await Sender.Send(command);

        return result.Match(
            Right: session => Ok(new { SessionId = session.Id.Value }),
            Left: exception => exception switch
            {
                KeyNotFoundException => NotFound(new { error = exception.Message }),
                UnauthorizedAccessException => Unauthorized(new { error = exception.Message }),
                InvalidOperationException => BadRequest(new { error = exception.Message }),
                _ => StatusCode(500, new { error = exception.Message })
            });
    }

    [HttpPost("{sessionId}/answer")]
    public async Task<IActionResult> SubmitAnswer(Guid sessionId, [FromBody] SubmitAnswerRequest request)
    {
        var command = new SubmitAnswerCommand
        {
            SessionId = sessionId,
            QuestionId = request.QuestionId,
            SelectedOptionIds = request.SelectedOptionIds ?? new List<Guid>(),
            TextAnswer = request.TextAnswer
        };

        var result = await Sender.Send(command);

        return result.Match(
            Right: _ => Ok(new { success = true }),
            Left: exception => exception switch
            {
                KeyNotFoundException => NotFound(new { error = exception.Message }),
                UnauthorizedAccessException => Unauthorized(new { error = exception.Message }),
                InvalidOperationException => BadRequest(new { error = exception.Message }),
                _ => StatusCode(500, new { error = exception.Message })
            });
    }

    [HttpPost("{sessionId}/complete")]
    public async Task<IActionResult> CompleteSession(Guid sessionId)
    {
        var command = new CompleteTestSessionCommand
        {
            SessionId = sessionId
        };var result = await Sender.Send(command);

    return result.Match(
        Right: session => Ok(new
        {
            SessionId = session.Id.Value,
            TotalScore = session.TotalScore,
            Status = session.Status.ToString()
        }),
        Left: exception => exception switch
        {
            KeyNotFoundException => NotFound(new { error = exception.Message }),
            UnauthorizedAccessException => Unauthorized(new { error = exception.Message }),
            InvalidOperationException => BadRequest(new { error = exception.Message }),
            _ => StatusCode(500, new { error = exception.Message })
        });
}

[HttpGet("test/{testId}/active")]
[Authorize(Roles = "Teacher,Admin,Manager")]
public async Task<IActionResult> GetActiveSessions(Guid testId)
{
    var query = new GetActiveTestSessionsQuery
    {
        TestId = testId
    };

    var result = await Sender.Send(query);

    return result.Match(
        Right: sessions => Ok(sessions.Select(s => new
        {
            SessionId = s.Id.Value,
            StudentId = s.StudentId,
            StartedAt = s.StartedAt,
            AnswersCount = s.Answers.Count,
            ViolationsCount = s.Violations.Count
        })),
        Left: exception => exception switch
        {
            KeyNotFoundException => NotFound(new { error = exception.Message }),
            UnauthorizedAccessException => Unauthorized(new { error = exception.Message }),
            _ => StatusCode(500, new { error = exception.Message })
        });
}

[HttpGet("my/{testId}")]
public async Task<IActionResult> GetMySession(Guid testId)
{
    var query = new GetMyTestSessionQuery
    {
        TestId = testId
    };

    var result = await Sender.Send(query);

    return result.Match(
        Right: session => session != null 
            ? Ok(new
            {
                SessionId = session.Id.Value,
                Status = session.Status.ToString(),
                StartedAt = session.StartedAt,
                FinishedAt = session.FinishedAt,
                TotalScore = session.TotalScore
            })
            : NotFound(),
        Left: exception => exception switch
        {
            UnauthorizedAccessException => Unauthorized(new { error = exception.Message }),
            _ => StatusCode(500, new { error = exception.Message })
        });
}
}