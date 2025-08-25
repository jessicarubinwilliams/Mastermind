using Mastermind.Api.Models;
using Mastermind.Core.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mastermind.Api.Controllers.TestControllers;

/// <summary>
/// Endpoints for ad hoc validation of the caching service.
/// These endpoints are intended for manual testing from Swagger during development.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TestCacheController(ICacheService cache) : ControllerBase
{
    private readonly ICacheService _cache = cache;

    /// <summary>
    /// Gets a payload from cache by key or creates it if missing.
    /// </summary>
    /// <remarks>
    /// On a cache miss this endpoint uses a simple factory to create a payload that records the current UTC timestamp.
    /// Use the optional query parameters to apply absolute and sliding expirations for manual tests.
    /// A query value of zero means do not apply that expiration and allow defaults to take effect.
    /// </remarks>
    /// <param name="key">The cache key. Use colon delimited segments such as test:cache:ping.</param>
    /// <param name="absoluteExpirationSeconds">Optional absolute expiration in seconds. Use zero to disable.</param>
    /// <param name="slidingExpirationSeconds">Optional sliding expiration in seconds. Use zero to disable.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>The cached or newly created <see cref="TestCachePayload" />.</returns>
    [HttpGet("get-or-create/{key}")]
    [ProducesResponseType(typeof(TestCachePayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrCreateAsync(
        string key,
        [FromQuery] int? absoluteExpirationSeconds = null,
        [FromQuery] int? slidingExpirationSeconds = null,
        CancellationToken ct = default)
    {
        if (absoluteExpirationSeconds is < 0 || slidingExpirationSeconds is < 0)
            return BadRequest("Expiration seconds must be greater than or equal to zero.");

        var absolute = ToTimeSpanOrNull(absoluteExpirationSeconds);
        var sliding = ToTimeSpanOrNull(slidingExpirationSeconds);

        var value = await _cache.GetOrCreateAsync(
            key,
            async _ =>
            {
                await Task.Yield();
                return new TestCachePayload
                {
                    WhenUtc = DateTimeOffset.UtcNow,
                    Note = "created by factory"
                };
            },
            absolute,
            sliding,
            ct);

        return Ok(value);
    }

    /// <summary>
    /// Writes a payload to cache under the provided key.
    /// </summary>
    /// <remarks>
    /// Use this endpoint to pre-seed entries or to validate serialization and deserialization behavior.
    /// Optional query parameters apply absolute and sliding expirations for manual tests.
    /// A query value of zero means do not apply that expiration and allow defaults to take effect.
    /// </remarks>
    /// <param name="key">The cache key. Use colon delimited segments such as test:cache:small.</param>
    /// <param name="payload">The payload to store.</param>
    /// <param name="absoluteExpirationSeconds">Optional absolute expiration in seconds. Use zero to disable.</param>
    /// <param name="slidingExpirationSeconds">Optional sliding expiration in seconds. Use zero to disable.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    [HttpPost("set/{key}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAsync(
        string key,
        [FromBody] TestCachePayload payload,
        [FromQuery] int? absoluteExpirationSeconds = null,
        [FromQuery] int? slidingExpirationSeconds = null,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest("Payload is required.");

        if (absoluteExpirationSeconds is < 0 || slidingExpirationSeconds is < 0)
            return BadRequest("Expiration seconds must be greater than or equal to zero.");

        var absolute = ToTimeSpanOrNull(absoluteExpirationSeconds);
        var sliding = ToTimeSpanOrNull(slidingExpirationSeconds);

        await _cache.SetAsync(
            key,
            payload,
            absolute,
            sliding,
            ct);

        return NoContent();
    }

    /// <summary>
    /// Gets a payload from cache by key without creating it on a miss.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>The cached <see cref="TestCachePayload" /> if present.</returns>
    [HttpGet("get/{key}")]
    [ProducesResponseType(typeof(TestCachePayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(string key, CancellationToken ct)
    {
        var value = await _cache.GetAsync<TestCachePayload>(key, ct);
        if (value is null) return NotFound();
        return Ok(value);
    }

    /// <summary>
    /// Removes a cache entry by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    [HttpDelete("remove/{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveAsync(string key, CancellationToken ct)
    {
        await _cache.RemoveAsync(key, ct);
        return NoContent();
    }

    /// <summary>
    /// Converts a nullable number of seconds to a nullable <see cref="TimeSpan" />.
    /// </summary>
    /// <param name="seconds">Seconds to convert. Null means not provided. Zero means disabled.</param>
    /// <returns>
    /// Null to indicate no explicit expiration value was provided.
    /// <br />Zero to indicate the caller explicitly disabled that expiration.
    /// <br />A positive <see cref="TimeSpan" /> otherwise.
    /// </returns>
    private TimeSpan? ToTimeSpanOrNull(int? seconds)
    {
        // Interpret null as not provided
        if (seconds is null) return null;

        // A value of zero means do not apply that expiration and let defaults handle it
        if (seconds.Value == 0) return TimeSpan.Zero;

        // Positive values convert to a TimeSpan
        return TimeSpan.FromSeconds(seconds.Value);
    }
}