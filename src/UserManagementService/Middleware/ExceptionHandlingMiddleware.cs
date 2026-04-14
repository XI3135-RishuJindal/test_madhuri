using System.Net;
using System.Text.Json;
using UserManagementService.Exceptions;
using UserManagementService.Models.DTOs;

namespace UserManagementService.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        var response = context.Response;
        response.ContentType = "application/json";

        ErrorResponse errorResponse;

        switch (exception)
        {
            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = ErrorResponse.Create("Conflict", conflictEx.Message, 409);
                _logger.LogWarning(conflictEx, "Conflict error: {Message}", conflictEx.Message);
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = ErrorResponse.Create("NotFound", notFoundEx.Message, 404);
                _logger.LogWarning(notFoundEx, "Not found error: {Message}", notFoundEx.Message);
                break;

            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ErrorResponse.CreateValidationError(validationEx.Errors);
                _logger.LogWarning(validationEx, "Validation error: {Message}", validationEx.Message);
                break;

            case UnauthorizedAccessException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = ErrorResponse.Create("Unauthorized", unauthorizedEx.Message, 401);
                _logger.LogWarning(unauthorizedEx, "Unauthorized error: {Message}", unauthorizedEx.Message);
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ErrorResponse.Create("BadRequest", argEx.Message, 400);
                _logger.LogWarning(argEx, "Argument error: {Message}", argEx.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = ErrorResponse.Create(
                    "InternalServerError", 
                    "An unexpected error occurred. Please try again later.", 
                    500);
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var result = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(result);
    }
}
