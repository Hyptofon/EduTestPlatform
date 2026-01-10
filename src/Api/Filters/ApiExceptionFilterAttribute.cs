using Application.Authentication.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    public ApiExceptionFilterAttribute()
    {
        // Реєструємо обробники для різних типів помилок
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(AuthenticationException), HandleAuthenticationException } // На випадок якщо десь кинеться throw
        };
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        Type type = context.Exception.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {
            _exceptionHandlers[type].Invoke(context);
            return;
        }
        
        // Можна додати обробку спадкоємців
        foreach(var handler in _exceptionHandlers) {
             if (type.IsSubclassOf(handler.Key)) {
                 handler.Value.Invoke(context);
                 return;
             }
        }
    }

    private void HandleValidationException(ExceptionContext context)
    {
        var exception = (ValidationException)context.Exception;

        var details = new ValidationProblemDetails(
            exception.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failure => failure.Key, failure => failure.ToArray()))
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }
    
    private void HandleAuthenticationException(ExceptionContext context)
    {
        var exception = (AuthenticationException)context.Exception;
        
        var details = new ProblemDetails
        {
            Title = "Authentication Error",
            Detail = exception.Message
        };

        context.Result = new ObjectResult(details) { StatusCode = 400 };
        context.ExceptionHandled = true;
    }
}