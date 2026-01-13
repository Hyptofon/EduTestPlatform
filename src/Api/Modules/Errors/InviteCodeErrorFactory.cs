using Application.Invites.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class InviteCodeErrorFactory
{
    public static ObjectResult ToObjectResult(this InviteCodeException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                InviteCodeNotFoundException => StatusCodes.Status404NotFound,
                InviteCodeNotFoundByCodeException => StatusCodes.Status404NotFound,
                InviteCodeAlreadyExistsException => StatusCodes.Status409Conflict,
                InviteCodeNotValidException => StatusCodes.Status400BadRequest,
                UnhandledInviteCodeException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Invite code error handler not implemented")
            }
        };
    }
}