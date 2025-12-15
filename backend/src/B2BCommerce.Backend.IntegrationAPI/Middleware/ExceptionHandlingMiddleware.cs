using System.Net;
using System.Text.Json;
using B2BCommerce.Backend.Application.Exceptions;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.IntegrationAPI.Middleware;

/// <summary>
/// Global exception handling middleware that catches exceptions and returns appropriate HTTP responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateErrorResponse("Not Found", notFoundEx.Message)
            ),
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateValidationErrorResponse(validationEx.Errors)
            ),
            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                CreateErrorResponse("Forbidden", forbiddenEx.Message)
            ),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                CreateErrorResponse("Conflict", conflictEx.Message)
            ),
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse("Domain Error", domainEx.Message)
            ),
            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                CreateErrorResponse("Unauthorized", unauthorizedEx.Message)
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse("Bad Request", argEx.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                CreateInternalErrorResponse(exception)
            )
        };

        LogException(exception, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private void LogException(Exception exception, HttpStatusCode statusCode)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.BadRequest or HttpStatusCode.NotFound => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(
            logLevel,
            exception,
            "Integration API Exception: {ExceptionType} - {Message}",
            exception.GetType().Name,
            exception.Message);
    }

    private object CreateErrorResponse(string title, string message)
    {
        return new
        {
            Success = false,
            Error = new
            {
                Title = title,
                Message = message
            }
        };
    }

    private object CreateValidationErrorResponse(IReadOnlyDictionary<string, string[]> errors)
    {
        return new
        {
            Success = false,
            Error = new
            {
                Title = "Validation Error",
                Message = "One or more validation errors occurred.",
                Errors = errors
            }
        };
    }

    private object CreateInternalErrorResponse(Exception exception)
    {
        var response = new
        {
            Success = false,
            Error = new
            {
                Title = "Internal Server Error",
                Message = _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                StackTrace = _env.IsDevelopment() ? exception.StackTrace : null
            }
        };

        return response;
    }
}

/// <summary>
/// Extension methods for registering the exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
