using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserManagementService.Controllers;

/// <summary>
/// Controller for health check operations.
/// </summary>
[ApiController]
[Route("health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the health status of the service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health status.</returns>
    /// <response code="200">Service is healthy.</response>
    /// <response code="503">Service is unhealthy.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        _logger.LogDebug("GET /health");

        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.Select(e => new HealthCheckEntry
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration.TotalMilliseconds
            }).ToList()
        };

        if (report.Status == HealthStatus.Healthy)
        {
            return Ok(response);
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// Simple liveness probe.
    /// </summary>
    /// <returns>OK if service is alive.</returns>
    [HttpGet("live")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLiveness()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe checking dependencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OK if service is ready to accept traffic.</returns>
    [HttpGet("ready")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        if (report.Status == HealthStatus.Healthy)
        {
            return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, new 
        { 
            status = "not_ready", 
            timestamp = DateTime.UtcNow 
        });
    }
}

/// <summary>
/// Health check response model.
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// Overall health status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the health check.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Individual health check entries.
    /// </summary>
    public List<HealthCheckEntry> Checks { get; set; } = new();
}

/// <summary>
/// Individual health check entry.
/// </summary>
public class HealthCheckEntry
{
    /// <summary>
    /// Name of the health check.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status of the health check.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Description of the health check result.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Duration of the health check in milliseconds.
    /// </summary>
    public double Duration { get; set; }
}
