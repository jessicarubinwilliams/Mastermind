using System;

namespace Mastermind.Api.Models;

public class HealthStatusResponse
{
    /// <summary>
    /// A short status indicator.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The server timestamp in UTC when the status was generated.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; init; }
}