namespace Mastermind.Core.GamePlay.Models;

/// <summary>
/// Overall lifecycle state for a single game session.
/// </summary>
public enum GameStatus
{
    /// <summary>
    /// Game is active and accepting guesses.
    /// </summary>
    InProgress,

    /// <summary>
    /// Player has correctly guessed the secret.
    /// </summary>
    Won,

    /// <summary>
    /// Player has used all attempts without guessing the secret.
    /// </summary>
    Lost
}