using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagementService.Controllers;
using UserManagementService.Models;
using UserManagementService.Models.DTOs;
using UserManagementService.Services;
using Xunit;

namespace UserManagementService.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_userManagementServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserById_WhenUserExists_ReturnsOkWithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDTO
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Role = "User",
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagementServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUserById(userId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<UserDTO>(okResult.Value);
        Assert.Equal(userId, returnedUser.Id);
    }

    [Fact]
    public async Task GetUserById_WhenUserNotExists_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagementServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDTO?)null);

        // Act
        var result = await _controller.GetUserById(userId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal("NotFound", errorResponse.Error);
    }

    [Fact]
    public async Task CreateUser_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        var createdUser = new UserDTO
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Role = "User",
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _userManagementServiceMock.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(request, CancellationToken.None);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdAtResult.StatusCode);
        var returnedUser = Assert.IsType<UserDTO>(createdAtResult.Value);
        Assert.Equal(request.Name, returnedUser.Name);
    }

    [Fact]
    public async Task DeleteUser_WhenUserExists_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagementServiceMock.Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(userId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteUser_WhenUserNotExists_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagementServiceMock.Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(userId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal("NotFound", errorResponse.Error);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOkWithUserList()
    {
        // Arrange
        var pagination = new PaginationQuery { Page = 1, PageSize = 10 };
        var response = new UserListResponse
        {
            Users = new List<UserDTO>
            {
                new() { Id = Guid.NewGuid(), Name = "User 1", Email = "user1@example.com" },
                new() { Id = Guid.NewGuid(), Name = "User 2", Email = "user2@example.com" }
            },
            Pagination = Pagination.Create(2, 1, 10)
        };

        _userManagementServiceMock.Setup(x => x.GetAllUsersAsync(pagination, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAllUsers(pagination, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<UserListResponse>(okResult.Value);
        Assert.Equal(2, returnedResponse.Users.Count());
    }
}
