using System;

namespace Mastermind.Core.GamePlay.Models;

/// <summary>
/// A historical record of a single guess within a game.
/// </summary>
public record GuessEntry
{
    /// <summary>
    /// One based attempt number at the time the guess was submitted.
    /// </summary>
    public int AttemptNumber { get; init; }

    /// <summary>
    /// The four digit guess. Array length is expected to be exactly four.
    /// Values are expected to be within the configured digit range.
    /// </summary>
    public int[] Guess { get; init; } = [];

    /// <summary>
    /// Feedback computed for this guess.
    /// </summary>
    public Feedback Feedback { get; init; } = new();

    /// <summary>
    /// UTC timestamp when this guess was recorded.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; init; }
}