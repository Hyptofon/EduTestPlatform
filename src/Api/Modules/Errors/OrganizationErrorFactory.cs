using Application.Organizations.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class OrganizationErrorFactory
{
    public static ObjectResult ToObjectResult(this OrganizationException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                OrganizationNotFoundException => StatusCodes.Status404NotFound,
                OrganizationAlreadyExistsException => StatusCodes.Status409Conflict,
                OrganizationCannotBeDeletedException => StatusCodes.Status409Conflict,
                UnhandledOrganizationException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Organization error handler not implemented")
            }
        };
    }
}