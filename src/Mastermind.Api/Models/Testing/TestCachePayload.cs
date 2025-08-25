using System;

namespace Mastermind.Api.Models;

/// <summary>
/// Request and response model used for manual cache service testing.
/// </summary>
public class TestCachePayload
{
    /// <summary>
    /// UTC timestamp when the value was generated or stored.
    /// </summary>
    public DateTimeOffset WhenUtc { get; init; }

    /// <summary>
    /// Optional note to identify or describe the cached value.
    /// </summary>
    public string? Note { get; init; }
}