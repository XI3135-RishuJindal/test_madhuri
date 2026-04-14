using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Models.Entities;

namespace UserManagementService.Repositories;

/// <summary>
/// Repository implementation for verification code data access operations.
/// </summary>
public class VerificationCodeRepository : IVerificationCodeRepository
{
    private readonly UserDbContext _context;
    private readonly ILogger<VerificationCodeRepository> _logger;

    public VerificationCodeRepository(UserDbContext context, ILogger<VerificationCodeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<VerificationCode> CreateAsync(
        VerificationCode verificationCode, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating verification code for user: {UserId}, type: {Type}", 
            verificationCode.UserId, verificationCode.Type);

        verificationCode.Id = Guid.NewGuid();
        verificationCode.CreatedAt = DateTime.UtcNow;

        _context.VerificationCodes.Add(verificationCode);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Verification code created with ID: {CodeId}", verificationCode.Id);

        return verificationCode;
    }

    /// <inheritdoc />
    public async Task<VerificationCode?> GetValidCodeAsync(
        Guid userId, 
        string code, 
        string type, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Looking up verification code for user: {UserId}, type: {Type}", userId, type);

        return await _context.VerificationCodes
            .Where(vc => vc.UserId == userId 
                && vc.Code == code 
                && vc.Type == type 
                && !vc.IsUsed 
                && vc.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkAsUsedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Marking verification code as used: {CodeId}", id);

        var code = await _context.VerificationCodes.FindAsync(new object[] { id }, cancellationToken);
        
        if (code != null)
        {
            code.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task InvalidateExistingCodesAsync(
        Guid userId, 
        string type, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Invalidating existing codes for user: {UserId}, type: {Type}", userId, type);

        var existingCodes = await _context.VerificationCodes
            .Where(vc => vc.UserId == userId && vc.Type == type && !vc.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var code in existingCodes)
        {
            code.IsUsed = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
