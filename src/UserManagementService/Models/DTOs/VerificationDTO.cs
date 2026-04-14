using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models.DTOs;

/// <summary>
/// Request model for sending a verification code.
/// </summary>
public class VerificationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Type of verification: "email" or "password_reset"
    /// </summary>
    [Required]
    public string VerificationType { get; set; } = "email";
}

/// <summary>
/// Response model for verification operations.
/// </summary>
public class VerificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request model for validating a verification code.
/// </summary>
public class ValidateVerificationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}
