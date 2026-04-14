using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagementService.Models.DTOs;
using UserManagementService.Models.Entities;
using UserManagementService.Repositories;
using UserManagementService.Services;
using Xunit;

namespace UserManagementService.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _service = new AuthService(
            _userRepositoryMock.Object,
            _tokenServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var email = "test@test.com";
        var password = "Password123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = email,
            PasswordHash = passwordHash,
            Role = "User",
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var request = new LoginRequest { Email = email, Password = password };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Email, user.Role))
            .Returns("test-access-token");

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        _tokenServiceMock
            .Setup(x => x.GetAccessTokenExpirationSeconds())
            .Returns(3600);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@test.com", Password = "Password123!" };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var email = "test@test.com";
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true
        };

        var request = new LoginRequest { Email = email, Password = wrongPassword };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        var email = "test@test.com";
        var password = "Password123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsActive = false
        };

        var request = new LoginRequest { Email = email, Password = password };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LogoutAsync_RemovesRefreshTokenFromCache()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _service.LogoutAsync(userId);

        // Assert
        _cacheServiceMock.Verify(
            x => x.RemoveAsync($"refresh_token:{userId}", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
