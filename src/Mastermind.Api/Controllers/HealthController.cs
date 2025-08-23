using Microsoft.AspNetCore.Mvc;

namespace Mastermind.Api.Controllers;

/// <summary>
/// Simple health check controller to confirm the API is up.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns a basic status payload confirming the API is running.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            timestampUtc = DateTime.UtcNow
        });
    }
}
