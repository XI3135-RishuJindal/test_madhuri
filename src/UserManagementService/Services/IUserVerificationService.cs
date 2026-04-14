using UserManagementService.Models.DTOs;

namespace UserManagementService.Services;

/// <summary>
/// Service interface for user verification operations.
/// </summary>
public interface IUserVerificationService
{
    Task<VerificationResponse> SendVerificationCodeAsync(VerificationRequest request, CancellationToken cancellationToken = default);
    Task<VerificationResponse> ValidateVerificationCodeAsync(ValidateVerificationRequest request, CancellationToken cancellationToken = default);
}
