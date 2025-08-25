namespace Mastermind.Core.GamePlay.Models;

/// <summary>
/// Feedback for a single guess attempt.
/// </summary>
public record Feedback
{
    /// <summary>
    /// Count of digits that match the secret at the same index position.
    /// </summary>
    public int CorrectPositions { get; init; }

    /// <summary>
    /// Count of digits that appear in the secret regardless of position.
    /// Includes the digits also counted in <see cref="CorrectPositions" />.
    /// </summary>
    public int CorrectNumbers { get; init; }
}