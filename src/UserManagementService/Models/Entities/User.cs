using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementService.Models.Entities;

/// <summary>
/// User entity representing a user in the system.
/// </summary>
[Table("users")]
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// User's display name.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (unique).
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication.
    /// </summary>
    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's role in the system.
    /// </summary>
    [MaxLength(50)]
    [Column("role")]
    public string Role { get; set; } = "User";

    /// <summary>
    /// Indicates whether the user's email has been verified.
    /// </summary>
    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
