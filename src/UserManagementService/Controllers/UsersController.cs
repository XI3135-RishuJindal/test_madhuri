using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Models;
using UserManagementService.Models.DTOs;
using UserManagementService.Services;

namespace UserManagementService.Controllers;

/// <summary>
/// Controller for user management operations.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination.
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users with pagination metadata</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserListResponse>> GetAllUsers(
        [FromQuery] PaginationQuery pagination,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/users - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        var result = await _userManagementService.GetAllUsersAsync(pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDTO>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/users/{UserId}", id);

        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NotFound",
                Message = $"User with ID {id} not found.",
                StatusCode = 404
            });
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDTO>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/users - Email: {Email}", request.Email);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "ValidationError",
                Message = "Invalid request data.",
                StatusCode = 400,
                ValidationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
            });
        }

        try
        {
            var user = await _userManagementService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse
            {
                Error = "Conflict",
                Message = ex.Message,
                StatusCode = 409
            });
        }
    }

    /// <summary>
    /// Update an existing user.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDTO>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/v1/users/{UserId}", id);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "ValidationError",
                Message = "Invalid request data.",
                StatusCode = 400,
                ValidationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
            });
        }

        try
        {
            var user = await _userManagementService.UpdateUserAsync(id, request, cancellationToken);
            if (user == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NotFound",
                    Message = $"User with ID {id} not found.",
                    StatusCode = 404
                });
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse
            {
                Error = "Conflict",
                Message = ex.Message,
                StatusCode = 409
            });
        }
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DELETE /api/v1/users/{UserId}", id);

        var result = await _userManagementService.DeleteUserAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NotFound",
                Message = $"User with ID {id} not found.",
                StatusCode = 404
            });
        }

        return NoContent();
    }
}
