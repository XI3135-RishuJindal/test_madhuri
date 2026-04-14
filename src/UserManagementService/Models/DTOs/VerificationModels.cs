namespace UserManagementService.Models.DTOs;

/// <summary>
/// Request model for sending a verification code.
/// </summary>
public class VerificationRequest
{
    /// <summary>
    /// The user ID to send verification code to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of verification (e.g., "email", "password_reset").
    /// </summary>
    public string Type { get; set; } = "email";
}

/// <summary>
/// Response model for verification operations.
/// </summary>
public class VerificationResponse
{
    /// <summary>
    /// Indicates whether the verification was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the verification code expires (if applicable).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Request model for validating a verification code.
/// </summary>
public class ValidateVerificationRequest
{
    /// <summary>
    /// The user ID to validate verification code for.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The verification code to validate.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Type of verification (e.g., "email", "password_reset").
    /// </summary>
    public string Type { get; set; } = "email";
}
