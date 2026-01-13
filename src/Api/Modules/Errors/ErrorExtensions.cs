using Application.Invites.Exceptions;
using Application.Organizations.Exceptions;
using Application.OrganizationalUnits.Exceptions;
using Application.Subjects.Exceptions;
using Application.Tests.Exceptions;
using Application.TestSessions.Exceptions;
using Application.Authentication.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class ErrorExtensions
{
    public static ObjectResult ToObjectResult(this Exception exception)
    {
        return exception switch
        {
            OrganizationException orgEx => orgEx.ToObjectResult(),
            OrganizationalUnitException unitEx => unitEx.ToObjectResult(),
            InviteCodeException inviteEx => inviteEx.ToObjectResult(),
            SubjectException subjectEx => subjectEx.ToObjectResult(),
            TestException testEx => testEx.ToObjectResult(),
            TestSessionException sessionEx => sessionEx.ToObjectResult(),
            AuthenticationException authEx => authEx.ToObjectResult(),
            _ => new ObjectResult("An unexpected error occurred")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
    }
}