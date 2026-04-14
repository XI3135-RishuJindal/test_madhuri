namespace UserManagementService.Models;

/// <summary>
/// Standardized error response model.
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public IDictionary<string, string[]>? ValidationErrors { get; set; }
}
