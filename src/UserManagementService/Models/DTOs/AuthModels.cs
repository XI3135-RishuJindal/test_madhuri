namespace UserManagementService.Models.DTOs;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response model for successful authentication.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// User information.
    /// </summary>
    public UserDTO? User { get; set; }
}

/// <summary>
/// Request model for refreshing an access token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
