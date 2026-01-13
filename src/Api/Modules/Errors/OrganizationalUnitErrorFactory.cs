using Application.OrganizationalUnits.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class OrganizationalUnitErrorFactory
{
    public static ObjectResult ToObjectResult(this OrganizationalUnitException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                OrganizationalUnitNotFoundException => StatusCodes.Status404NotFound,
                OrganizationNotFoundForUnitException => StatusCodes.Status404NotFound,
                ParentUnitNotFoundException => StatusCodes.Status404NotFound,
                OrganizationalUnitCannotBeDeletedException => StatusCodes.Status409Conflict,
                UnhandledOrganizationalUnitException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Organizational unit error handler not implemented")
            }
        };
    }
}