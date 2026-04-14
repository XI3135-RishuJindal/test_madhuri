using UserManagementService.Models.DTOs;

namespace UserManagementService.Services;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Login response with JWT token.</returns>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="request">Refresh token request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New login response with refreshed tokens.</returns>
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a user's session/tokens.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
