using UserManagementService.Models.Entities;

namespace UserManagementService.Repositories;

/// <summary>
/// Repository interface for verification code data access operations.
/// </summary>
public interface IVerificationCodeRepository
{
    /// <summary>
    /// Creates a new verification code.
    /// </summary>
    /// <param name="verificationCode">Verification code entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created verification code.</returns>
    Task<VerificationCode> CreateAsync(VerificationCode verificationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a valid verification code for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="code">Verification code.</param>
    /// <param name="type">Verification type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Verification code if valid, null otherwise.</returns>
    Task<VerificationCode?> GetValidCodeAsync(Guid userId, string code, string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a verification code as used.
    /// </summary>
    /// <param name="id">Verification code ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkAsUsedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all existing codes for a user and type.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="type">Verification type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateExistingCodesAsync(Guid userId, string type, CancellationToken cancellationToken = default);
}
