using UserManagementService.Models.DTOs;
using UserManagementService.Repositories;

namespace UserManagementService.Services;

/// <summary>
/// Service implementation for authentication operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        ICacheService cacheService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
            return null;
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed - user is inactive: {Email}", request.Email);
            return null;
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - invalid password for: {Email}", request.Email);
            return null;
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresIn = _tokenService.GetAccessTokenExpirationSeconds();

        // Cache refresh token in Redis
        await _cacheService.SetAsync(
            $"refresh_token:{user.Id}", 
            refreshToken, 
            TimeSpan.FromDays(7), 
            cancellationToken);

        _logger.LogInformation("Login successful for user: {UserId}", user.Id);

        return new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            User = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            }
        };
    }

    /// <inheritdoc />
    public async Task<LoginResponse?> RefreshTokenAsync(
        RefreshTokenRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refresh token attempt");

        // Validate refresh token format and extract user ID
        var userId = _tokenService.ValidateRefreshToken(request.RefreshToken);
        
        if (userId == null)
        {
            _logger.LogWarning("Refresh token validation failed - invalid format");
            return null;
        }

        // Check if refresh token exists in cache
        var cachedToken = await _cacheService.GetAsync<string>(
            $"refresh_token:{userId}", 
            cancellationToken);

        if (cachedToken == null || cachedToken != request.RefreshToken)
        {
            _logger.LogWarning("Refresh token validation failed - token not found or mismatch");
            return null;
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Refresh token validation failed - user not found or inactive");
            return null;
        }

        // Generate new tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiresIn = _tokenService.GetAccessTokenExpirationSeconds();

        // Update cached refresh token
        await _cacheService.SetAsync(
            $"refresh_token:{user.Id}", 
            newRefreshToken, 
            TimeSpan.FromDays(7), 
            cancellationToken);

        _logger.LogInformation("Token refresh successful for user: {UserId}", user.Id);

        return new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            RefreshToken = newRefreshToken,
            User = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            }
        };
    }

    /// <inheritdoc />
    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logout for user: {UserId}", userId);

        // Remove refresh token from cache
        await _cacheService.RemoveAsync($"refresh_token:{userId}", cancellationToken);

        _logger.LogInformation("Logout successful for user: {UserId}", userId);
    }
}
