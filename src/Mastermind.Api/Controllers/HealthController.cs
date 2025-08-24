using System;
using Mastermind.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mastermind.Api.Controllers
{
    /// <summary>
    /// Simple health check endpoint to confirm the API is running.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns a basic status payload indicating the API is healthy.
        /// </summary>
        /// <returns>
        /// A <see cref="HealthStatusResponse"/> containing a status string and the current UTC timestamp.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            var payload = new HealthStatusResponse
            {
                Status = "Healthy",
                TimestampUtc = DateTime.UtcNow
            };

            return Ok(payload);
        }
    }
}