using UserManagementService.Models;

namespace UserManagementService.Services;

/// <summary>
/// Service interface for JWT token operations.
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
