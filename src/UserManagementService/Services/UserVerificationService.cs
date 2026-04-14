using System.Security.Cryptography;
using UserManagementService.Models.DTOs;
using UserManagementService.Repositories;

namespace UserManagementService.Services;

/// <summary>
/// Implementation of user verification operations (email verification, password reset).
/// </summary>
public class UserVerificationService : IUserVerificationService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserVerificationService> _logger;

    // Verification code expiration time
    private static readonly TimeSpan VerificationCodeExpiry = TimeSpan.FromMinutes(15);

    public UserVerificationService(
        IUserRepository userRepository,
        ICacheService cacheService,
        ILogger<UserVerificationService> logger)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<VerificationResponse> SendVerificationCodeAsync(VerificationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending verification code to: {Email}, Type: {Type}", request.Email, request.VerificationType);

        // Check if user exists
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            // Return success even if user doesn't exist (security best practice)
            _logger.LogWarning("Verification requested for non-existent email: {Email}", request.Email);
            return new VerificationResponse
            {
                Success = true,
                Message = "If the email exists, a verification code has been sent."
            };
        }

        // Generate verification code
        var code = GenerateVerificationCode();
        var cacheKey = $"verification:{request.VerificationType}:{request.Email.ToLower()}";

        // Store code in cache
        await _cacheService.SetAsync(cacheKey, code, VerificationCodeExpiry, cancellationToken);

        // TODO: Integrate with email service to send the verification code
        // For now, log the code (remove in production)
        _logger.LogInformation("Verification code generated for {Email}: {Code} (remove this log in production)", request.Email, code);

        return new VerificationResponse
        {
            Success = true,
            Message = "If the email exists, a verification code has been sent."
        };
    }

    public async Task<VerificationResponse> ValidateVerificationCodeAsync(ValidateVerificationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating verification code for: {Email}", request.Email);

        // Try email verification first, then password reset
        var emailCacheKey = $"verification:email:{request.Email.ToLower()}";
        var passwordResetCacheKey = $"verification:password_reset:{request.Email.ToLower()}";

        var storedCode = await _cacheService.GetAsync<string>(emailCacheKey, cancellationToken);
        var verificationType = "email";

        if (storedCode == null)
        {
            storedCode = await _cacheService.GetAsync<string>(passwordResetCacheKey, cancellationToken);
            verificationType = "password_reset";
        }

        if (storedCode == null)
        {
            _logger.LogWarning("No verification code found for: {Email}", request.Email);
            return new VerificationResponse
            {
                Success = false,
                Message = "Invalid or expired verification code."
            };
        }

        if (storedCode != request.Code)
        {
            _logger.LogWarning("Invalid verification code for: {Email}", request.Email);
            return new VerificationResponse
            {
                Success = false,
                Message = "Invalid or expired verification code."
            };
        }

        // Code is valid - remove it from cache
        await _cacheService.RemoveAsync(verificationType == "email" ? emailCacheKey : passwordResetCacheKey, cancellationToken);

        // If email verification, mark user as verified
        if (verificationType == "email")
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user != null)
            {
                user.EmailVerified = true;
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        _logger.LogInformation("Verification code validated successfully for: {Email}", request.Email);

        return new VerificationResponse
        {
            Success = true,
            Message = "Verification successful."
        };
    }

    private static string GenerateVerificationCode()
    {
        // Generate a 6-digit numeric code
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var code = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return code.ToString("D6");
    }
}
