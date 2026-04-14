using UserManagementService.Exceptions;
using UserManagementService.Models.DTOs;
using UserManagementService.Models.Entities;
using UserManagementService.Models.Events;
using UserManagementService.Repositories;

namespace UserManagementService.Services;

/// <summary>
/// Service implementation for user management operations.
/// </summary>
public class UserManagementServiceImpl : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserManagementServiceImpl> _logger;

    public UserManagementServiceImpl(
        IUserRepository userRepository,
        ILogger<UserManagementServiceImpl> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<UserDTO>> GetAllUsersAsync(
        PaginationQuery query, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching users - Page: {Page}, PageSize: {PageSize}", 
            query.Page, query.PageSize);

        var (users, totalCount) = await _userRepository.GetAllAsync(
            query.Skip, 
            query.PageSize, 
            cancellationToken);

        var userDtos = users.Select(MapToDto).ToList();

        return new PaginatedResponse<UserDTO>
        {
            Items = userDtos,
            Pagination = Pagination.Create(totalCount, query.Page, query.PageSize)
        };
    }

    /// <inheritdoc />
    public async Task<UserDTO?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching user by ID: {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return null;
        }

        return MapToDto(user);
    }

    /// <inheritdoc />
    public async Task<UserDTO> CreateUserAsync(
        CreateUserRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new user with email: {Email}", request.Email);

        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("Email already exists: {Email}", request.Email);
            throw new ConflictException($"A user with email '{request.Email}' already exists.");
        }

        // Create user entity
        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role ?? "User",
            IsVerified = false,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

        // TODO: Publish UserCreatedEvent to Kafka (confirm requirement with domain experts)
        await PublishUserCreatedEventAsync(createdUser);

        _logger.LogInformation("User created successfully: {UserId}", createdUser.Id);

        return MapToDto(createdUser);
    }

    /// <inheritdoc />
    public async Task<UserDTO?> UpdateUserAsync(
        Guid id, 
        UpdateUserRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user: {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User not found for update: {UserId}", id);
            return null;
        }

        // Check if email is being changed and if new email already exists
        if (!string.IsNullOrEmpty(request.Email) && 
            !request.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            {
                throw new ConflictException($"A user with email '{request.Email}' already exists.");
            }
            user.Email = request.Email.ToLower();
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            user.Name = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            user.Role = request.Role;
        }

        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User updated successfully: {UserId}", id);

        return MapToDto(updatedUser);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);

        var result = await _userRepository.DeleteAsync(id, cancellationToken);

        if (result)
        {
            _logger.LogInformation("User deleted successfully: {UserId}", id);
        }
        else
        {
            _logger.LogWarning("User not found for deletion: {UserId}", id);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Verifying user: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User not found for verification: {UserId}", userId);
            return false;
        }

        user.IsVerified = true;
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User verified successfully: {UserId}", userId);

        return true;
    }

    /// <summary>
    /// Maps a User entity to a UserDTO.
    /// </summary>
    private static UserDTO MapToDto(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Publishes a UserCreatedEvent to Kafka.
    /// TODO: Implement Kafka integration when requirements are confirmed.
    /// </summary>
    private Task PublishUserCreatedEventAsync(User user)
    {
        var userCreatedEvent = new UserCreatedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role
        };

        // TODO: Publish to Kafka
        _logger.LogDebug("UserCreatedEvent prepared for user: {UserId}. Kafka publishing not yet implemented.", user.Id);

        return Task.CompletedTask;
    }
}
