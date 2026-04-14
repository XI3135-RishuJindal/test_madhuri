using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Models;

namespace UserManagementService.Repositories;

/// <summary>
/// PostgreSQL implementation of user repository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching user by ID: {UserId}", id);
        return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching user by email: {Email}", email);
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching users - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var totalCount = await _context.Users.CountAsync(cancellationToken);
        var users = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new user with email: {Email}", user.Email);

        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", user.Id);

        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User updated successfully: {UserId}", user.Id);
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);

        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for deletion: {UserId}", id);
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User deleted successfully: {UserId}", id);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }
}
