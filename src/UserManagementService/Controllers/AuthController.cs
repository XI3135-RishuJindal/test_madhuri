using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Models;
using UserManagementService.Models.DTOs;
using UserManagementService.Services;

namespace UserManagementService.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IUserVerificationService _verificationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserManagementService userManagementService,
        IUserVerificationService verificationService,
        ILogger<AuthController> logger)
    {
        _userManagementService = userManagementService;
        _verificationService = verificationService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate a user and return a JWT token.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/auth/login - Email: {Email}", request.Email);

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

        var result = await _userManagementService.AuthenticateAsync(request, cancellationToken);
        if (result == null)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "Unauthorized",
                Message = "Invalid email or password.",
                StatusCode = 401
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Send a verification code to the user's email.
    /// </summary>
    /// <param name="request">Verification request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification response</returns>
    [HttpPost("verify/send")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerificationResponse>> SendVerificationCode(
        [FromBody] VerificationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/auth/verify/send - Email: {Email}", request.Email);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "ValidationError",
                Message = "Invalid request data.",
                StatusCode = 400
            });
        }

        var result = await _verificationService.SendVerificationCodeAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Validate a verification code.
    /// </summary>
    /// <param name="request">Validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification response</returns>
    [HttpPost("verify/validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerificationResponse>> ValidateVerificationCode(
        [FromBody] ValidateVerificationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/auth/verify/validate - Email: {Email}", request.Email);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "ValidationError",
                Message = "Invalid request data.",
                StatusCode = 400
            });
        }

        var result = await _verificationService.ValidateVerificationCodeAsync(request, cancellationToken);
        return Ok(result);
    }
}
