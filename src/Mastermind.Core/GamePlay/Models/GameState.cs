using System;
using System.Collections.Generic;

namespace Mastermind.Core.GamePlay.Models;

/// <summary>
/// Aggregate state for a single game session stored in cache.
/// </summary>
public class GameState
{
    /// <summary>
    /// Unique identifier for the game session.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The secret digit combination the player must guess.
    /// </summary>
    public int[] SecretCombination { get; init; } = [];

    /// <summary>
    /// Maximum number of guesses allowed.
    /// </summary>
    public int AttemptLimit { get; init; }

    /// <summary>
    /// Number of guesses already used.
    /// </summary>
    public int AttemptsUsed { get; set; }

    /// <summary>
    /// Current lifecycle status of the game.
    /// </summary>
    public GameStatus Status { get; set; } = GameStatus.InProgress;

    /// <summary>
    /// UTC timestamp when the game was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// UTC timestamp when the game was last touched by any server side operation.
    /// </summary>
    public DateTimeOffset LastTouchedUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// UTC timestamp when the game reached a terminal state.
    /// Null while the game is in progress.
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>
    /// Chronological history of all guesses submitted in this game.
    /// </summary>
    public List<GuessEntry> GuessHistory { get; set; } = new();
}