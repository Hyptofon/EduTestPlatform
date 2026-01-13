using Application.Subjects.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class SubjectErrorFactory
{
    public static ObjectResult ToObjectResult(this SubjectException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                SubjectNotFoundException => StatusCodes.Status404NotFound,
                SubjectAccessDeniedException => StatusCodes.Status403Forbidden,
                InvalidAccessKeyException => StatusCodes.Status401Unauthorized,
                AlreadyEnrolledException => StatusCodes.Status409Conflict,
                NotEnrolledException => StatusCodes.Status400BadRequest,
                UnhandledSubjectException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Subject error handler not implemented")
            }
        };
    }
}