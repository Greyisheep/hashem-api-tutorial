using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Models;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get API health status
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> Get()
    {
        _logger.LogInformation("Health check requested");

        var healthData = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("O"),
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        };

        return Ok(ApiResponse.SuccessResponse(healthData, "API is healthy"));
    }
} 