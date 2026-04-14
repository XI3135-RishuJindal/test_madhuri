namespace UserManagementService.Models.DTOs;

/// <summary>
/// Standard error response model as per specification.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error type/code.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Optional validation errors for field-level issues.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new error response.
    /// </summary>
    public static ErrorResponse Create(string error, string message, int statusCode)
    {
        return new ErrorResponse
        {
            Error = error,
            Message = message,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates a validation error response.
    /// </summary>
    public static ErrorResponse CreateValidationError(Dictionary<string, string[]> validationErrors)
    {
        return new ErrorResponse
        {
            Error = "ValidationError",
            Message = "One or more validation errors occurred.",
            StatusCode = 400,
            ValidationErrors = validationErrors
        };
    }
}
