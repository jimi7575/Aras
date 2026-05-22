using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aras.Filters;

public sealed class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (statusCode, title) = context.Exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, context.Exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, context.Exception.Message),
            HttpRequestException => (StatusCodes.Status502BadGateway, "External service request failed."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(context.Exception, "Unhandled API exception.");
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = title
        })
        {
            StatusCode = statusCode
        };
        context.ExceptionHandled = true;
    }
}
