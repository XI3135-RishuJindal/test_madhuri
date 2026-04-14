using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models.DTOs;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response model for successful authentication.
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UserDTO User { get; set; } = new();
}
