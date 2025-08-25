using Mastermind.Core.GamePlay.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mastermind.Core.GamePlay;

/// <summary>
/// Core gameplay operations for Mastermind.
/// </summary>
public interface IGamePlayService
{
    /// <summary>
    /// Creates a new game and persists it to the cache.
    /// </summary>
    Task<GameState> CreateGameAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a guess for the specified game and returns the computed feedback entry.
    /// </summary>
    Task<GuessEntry> SubmitGuessAsync(Guid gameId, int[] guess, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current game state by identifier.
    /// </summary>
    Task<GameState> GetGameAsync(Guid gameId, CancellationToken cancellationToken = default);
}