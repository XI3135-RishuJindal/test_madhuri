using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagementService.Controllers;
using UserManagementService.Models.DTOs;
using UserManagementService.Services;
using Xunit;

namespace UserManagementService.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        var loginResponse = new LoginResponse
        {
            AccessToken = "test-access-token",
            RefreshToken = "test-refresh-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            User = new UserDTO
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = request.Email
            }
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        response.AccessToken.Should().Be("test-access-token");
        response.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword!"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorResponse = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithNewToken()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        };

        var loginResponse = new LoginResponse
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        _authServiceMock
            .Setup(x => x.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.RefreshToken(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        response.AccessToken.Should().Be("new-access-token");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        _authServiceMock
            .Setup(x => x.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.RefreshToken(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
