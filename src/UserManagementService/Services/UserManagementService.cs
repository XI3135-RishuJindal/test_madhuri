using System.Security.Cryptography;
using System.Text;
using UserManagementService.Models;
using UserManagementService.Models.DTOs;
using UserManagementService.Repositories;

namespace UserManagementService.Services;

/// <summary>
/// Implementation of user management business logic.
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRepository userRepository,
        ITokenService tokenService,
        ICacheService cacheService,
        ILogger<UserManagementService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", id);

        // Try cache first
        var cacheKey = $"user:{id}";
        var cachedUser = await _cacheService.GetAsync<UserDTO>(cacheKey, cancellationToken);
        if (cachedUser != null)
        {
            _logger.LogDebug("User found in cache: {UserId}", id);
            return cachedUser;
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return null;
        }

        var userDto = MapToDto(user);
        await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(5), cancellationToken);

        return userDto;
    }

    public async Task<UserListResponse> GetAllUsersAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        var (users, totalCount) = await _userRepository.GetAllAsync(pagination.Page, pagination.PageSize, cancellationToken);

        return new UserListResponse
        {
            Users = users.Select(MapToDto),
            Pagination = Pagination.Create(totalCount, pagination.Page, pagination.PageSize)
        };
    }

    public async Task<UserDTO> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new user with email: {Email}", request.Email);

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = HashPassword(request.Password),
            Role = "User",
            EmailVerified = false
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

        // TODO: Publish UserCreatedEvent to Kafka (confirm requirement with domain experts)
        _logger.LogInformation("User created successfully: {UserId}", createdUser.Id);

        return MapToDto(createdUser);
    }

    public async Task<UserDTO?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user: {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return null;
        }

        // Check if new email already exists (if email is being changed)
        if (!string.IsNullOrEmpty(request.Email) && 
            request.Email.ToLower() != user.Email.ToLower() &&
            await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email.ToLower();

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = HashPassword(request.Password);

        if (!string.IsNullOrEmpty(request.Role))
            user.Role = request.Role;

        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"user:{id}", cancellationToken);

        _logger.LogInformation("User updated successfully: {UserId}", id);
        return MapToDto(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);

        var result = await _userRepository.DeleteAsync(id, cancellationToken);

        if (result)
        {
            // Invalidate cache
            await _cacheService.RemoveAsync($"user:{id}", cancellationToken);
            _logger.LogInformation("User deleted successfully: {UserId}", id);
        }

        return result;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Authenticating user: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Authentication failed for email: {Email}", request.Email);
            return null;
        }

        var token = _tokenService.GenerateToken(user);
        var expiresIn = 3600; // 1 hour

        // Cache the token
        await _cacheService.SetAsync($"token:{user.Id}", token, TimeSpan.FromSeconds(expiresIn), cancellationToken);

        _logger.LogInformation("User authenticated successfully: {UserId}", user.Id);

        return new LoginResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            User = MapToDto(user)
        };
    }

    public async Task<bool> VerifyUserEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Verifying email for user: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.EmailVerified = true;
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"user:{userId}", cancellationToken);

        _logger.LogInformation("Email verified for user: {UserId}", userId);
        return true;
    }

    private static UserDTO MapToDto(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
