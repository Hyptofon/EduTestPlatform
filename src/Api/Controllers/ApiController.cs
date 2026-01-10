using System.Security.Claims;
using Application.Authentication.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;

    protected Guid UserId => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) 
        ? userId 
        : Guid.Empty;

    // Змінили тип параметра result на Exception (замість AuthenticationException), щоб switch міг зловити ValidationException
    protected IActionResult HandleResult<T>(LanguageExt.Either<AuthenticationException, T> result)
    {
        return result.Match(
            Right: response => Ok(response),
            Left: exception =>
            {
                // Тут ми кастимо exception до типу Exception для switch pattern matching
                return (Exception)exception switch
                {
                    UserNotFoundException => NotFound(CreateProblemDetails("Not Found", exception.Message)),
                    InvalidCredentialsException => Unauthorized(CreateProblemDetails("Unauthorized", exception.Message)),
                    UserAlreadyExistsException => Conflict(CreateProblemDetails("Conflict", exception.Message)),
                    InvalidInviteCodeException => BadRequest(CreateProblemDetails("Bad Request", exception.Message)),
                    TokenRefreshFailedException => BadRequest(CreateProblemDetails("Bad Request", exception.Message)),
                    
                    // ValidationException не наслідується від AuthenticationException, але якщо він якось сюди потрапить (через pipeline), ми його зловимо
                    // В нашій архітектурі ValidationException зазвичай викидається як Exception, а не повертається як Left.
                    // Але якщо ти хочеш ловити його тут, він має бути загорнутий в Left.
                    // Поки що залишимо так, це буде працювати для наших AuthenticationExceptions.
                    
                    _ => StatusCode(500, CreateProblemDetails("Internal Server Error", exception.Message))
                };
            });
    }

    private ProblemDetails CreateProblemDetails(string title, string detail)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = title switch 
            {
                "Not Found" => 404,
                "Unauthorized" => 401,
                "Conflict" => 409,
                "Bad Request" => 400,
                _ => 500
            }
        };
    }
}