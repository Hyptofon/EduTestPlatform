using Application.Users.Commands;
using Application.Users.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class UserErrorFactory
{
    public static ObjectResult ToObjectResult(this UserException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                UserNotFoundException => StatusCodes.Status404NotFound,
                UserNotAuthenticatedException => StatusCodes.Status401Unauthorized,
                UserNotInOrganizationException => StatusCodes.Status403Forbidden,
                UserAlreadyInOrganizationException => StatusCodes.Status409Conflict,
                InvalidInviteCodeForJoinException => StatusCodes.Status400BadRequest,
                UnhandledUserException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("User error handler not implemented")
            }
        };
    }
}
