using Application.TestSessions.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class TestSessionErrorFactory
{
    public static ObjectResult ToObjectResult(this TestSessionException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                TestSessionNotFoundException => StatusCodes.Status404NotFound,
                TestSessionAlreadyExistsException => StatusCodes.Status409Conflict,
                MaxAttemptsReachedException => StatusCodes.Status400BadRequest,
                TestSessionNotInProgressException => StatusCodes.Status400BadRequest,
                UnauthorizedSessionAccessException => StatusCodes.Status403Forbidden,
                UnhandledTestSessionException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Test session error handler not implemented")
            }
        };
    }
}