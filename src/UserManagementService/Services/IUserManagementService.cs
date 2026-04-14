using UserManagementService.Models;
using UserManagementService.Models.DTOs;

namespace UserManagementService.Services;

/// <summary>
/// Service interface for user management operations.
/// </summary>
public interface IUserManagementService
{
    Task<UserDTO?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserListResponse> GetAllUsersAsync(PaginationQuery pagination, CancellationToken cancellationToken = default);
    Task<UserDTO> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDTO?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> VerifyUserEmailAsync(Guid userId, CancellationToken cancellationToken = default);
}
