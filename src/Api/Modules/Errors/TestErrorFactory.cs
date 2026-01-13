using Application.Tests.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class TestErrorFactory
{
    public static ObjectResult ToObjectResult(this TestException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                TestNotFoundException => StatusCodes.Status404NotFound,
                TestNotAccessibleException => StatusCodes.Status403Forbidden,
                InvalidTestContentException => StatusCodes.Status400BadRequest,
                UnhandledTestException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Test error handler not implemented")
            }
        };
    }
}