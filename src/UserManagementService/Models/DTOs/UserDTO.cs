using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models.DTOs;

/// <summary>
/// Data transfer object for user information (excludes sensitive data).
/// </summary>
public class UserDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for creating a new user.
/// </summary>
public class CreateUserRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating an existing user.
/// </summary>
public class UpdateUserRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MinLength(8)]
    public string? Password { get; set; }

    [MaxLength(50)]
    public string? Role { get; set; }
}

/// <summary>
/// Response model for user list with pagination.
/// </summary>
public class UserListResponse
{
    public IEnumerable<UserDTO> Users { get; set; } = Enumerable.Empty<UserDTO>();
    public Pagination Pagination { get; set; } = new();
}
