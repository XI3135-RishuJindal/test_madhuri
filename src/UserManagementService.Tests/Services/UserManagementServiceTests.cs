using Microsoft.Extensions.Logging;
using Moq;
using UserManagementService.Models;
using UserManagementService.Models.DTOs;
using UserManagementService.Repositories;
using UserManagementService.Services;
using Xunit;

namespace UserManagementService.Tests.Services;

public class UserManagementServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<UserManagementService.Services.UserManagementService>> _loggerMock;
    private readonly IUserManagementService _service;

    public UserManagementServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<UserManagementService.Services.UserManagementService>>();

        _service = new UserManagementService.Services.UserManagementService(
            _userRepositoryMock.Object,
            _tokenServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUserDTO()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Role = "User",
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _cacheServiceMock.Setup(x => x.GetAsync<UserDTO>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDTO?)null);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotExists_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _cacheServiceMock.Setup(x => x.GetAsync<UserDTO>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDTO?)null);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidRequest_ReturnsCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _service.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New User", result.Name);
        Assert.Equal("newuser@example.com", result.Email);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "New User",
            Email = "existing@example.com",
            Password = "password123"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(request));
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteUserAsync(userId);

        // Assert
        Assert.True(result);
        _cacheServiceMock.Verify(x => x.RemoveAsync($"user:{userId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "75K3eLr+dx6JJFuJ7LwIpEpOFmwGZZkRiB97NrVFDXA=", // SHA256 of "password123"
            Role = "User",
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("test-jwt-token");

        // Act
        var result = await _service.AuthenticateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-jwt-token", result.Token);
        Assert.Equal("Bearer", result.TokenType);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.AuthenticateAsync(request);

        // Assert
        Assert.Null(result);
    }
}
