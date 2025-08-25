using System;

namespace Mastermind.Api.Models.GamePlay;

public class GuessDTO
{
    /// <summary>Attempt number starting at 1.</summary>
    public int Attempt { get; set; }

    /// <summary>The guess that was evaluated.</summary>
    public int[] Guess { get; set; } = Array.Empty<int>();

    /// <summary>Count of correct numbers regardless of position.</summary>
    public int CorrectNumbers { get; set; }

    /// <summary>Count of positions that were exactly correct.</summary>
    public int CorrectPositions { get; set; }

    /// <summary>UTC timestamp when the guess was recorded.</summary>
    public DateTimeOffset AtUtc { get; set; }
}