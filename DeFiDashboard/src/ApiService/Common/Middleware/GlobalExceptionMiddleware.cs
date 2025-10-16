using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Common.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns consistent error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        // Log the exception
        _logger.LogError(
            exception,
            "Unhandled exception occurred. Path: {Path}, Method: {Method}, TraceId: {TraceId}",
            context.Request.Path,
            context.Request.Method,
            context.TraceIdentifier);

        // Determine status code and error details based on exception type
        HttpStatusCode statusCode;
        string errorCode;
        string message;
        object? details;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                message = "Validation failed";
                details = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            case DbUpdateException dbEx:
                statusCode = HttpStatusCode.Conflict;
                errorCode = "DATABASE_ERROR";
                message = "A database error occurred";
                details = _environment.IsDevelopment()
                    ? new { detail = dbEx.InnerException?.Message ?? dbEx.Message }
                    : null;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                message = "Unauthorized access";
                details = null;
                break;

            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "INVALID_ARGUMENT";
                message = argEx.Message;
                details = null;
                break;

            case InvalidOperationException invOpEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "INVALID_OPERATION";
                message = invOpEx.Message;
                details = null;
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                message = "The requested resource was not found";
                details = null;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorCode = "INTERNAL_SERVER_ERROR";
                message = "An internal server error occurred";
                details = _environment.IsDevelopment()
                    ? new { detail = exception.Message, stackTrace = exception.StackTrace }
                    : null;
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = new
            {
                code = errorCode,
                message = message,
                traceId = context.TraceIdentifier,
                details = details
            }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsJsonAsync(response, options);
    }
}
