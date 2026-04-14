using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementService.Models.Entities;

/// <summary>
/// Entity for storing user verification codes.
/// </summary>
[Table("verification_codes")]
public class VerificationCode
{
    /// <summary>
    /// Unique identifier for the verification code record.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The user ID this verification code belongs to.
    /// </summary>
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// The verification code value.
    /// </summary>
    [Required]
    [MaxLength(10)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Type of verification (e.g., "email", "password_reset").
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("type")]
    public string Type { get; set; } = "email";

    /// <summary>
    /// Timestamp when the code was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the code expires.
    /// </summary>
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indicates whether the code has been used.
    /// </summary>
    [Column("is_used")]
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
