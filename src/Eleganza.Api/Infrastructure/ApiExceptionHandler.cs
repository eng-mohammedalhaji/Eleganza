using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Api.Infrastructure;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found.", exception.Message),
            UnauthorizedAccessException when httpContext.User.Identity?.IsAuthenticated == true
                => (StatusCodes.Status403Forbidden, "Forbidden.", exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication required.", exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation rejected.", exception.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "The resource changed. Retry the operation.", null),
            DbUpdateException => (StatusCodes.Status409Conflict, "The request conflicts with existing data.", null),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error.", "An unexpected error occurred."),
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled API exception for {Path}.", httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation(exception, "Handled API exception with status {StatusCode}.", status);
        }

        httpContext.Response.StatusCode = status;
        await problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path,
            },
        });
        return true;
    }
}
