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
        var (status, title, detail, code) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", exception.Message, "VALIDATION_ERROR"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found.", exception.Message, "RESOURCE_NOT_FOUND"),
            UnauthorizedAccessException when httpContext.User.Identity?.IsAuthenticated == true
                => (StatusCodes.Status403Forbidden, "Forbidden.", exception.Message, "FORBIDDEN"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication required.", exception.Message, "AUTHENTICATION_REQUIRED"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation rejected.", exception.Message, "BUSINESS_RULE_VIOLATION"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "The resource changed. Retry the operation.", "Reload the resource and retry.", "CONCURRENCY_CONFLICT"),
            DbUpdateException => (StatusCodes.Status409Conflict, "The request conflicts with existing data.", "The request conflicts with existing data.", "DATA_CONFLICT"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error.", "An unexpected error occurred.", "INTERNAL_ERROR"),
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
        httpContext.Response.ContentType = "application/problem+json";
        await problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["code"] = code,
                    ["traceId"] = httpContext.TraceIdentifier,
                },
            },
        });
        return true;
    }
}
