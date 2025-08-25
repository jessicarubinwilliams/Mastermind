using System;
using System.Collections.Generic;

namespace Mastermind.Api.Models.GamePlay;

public class GameResponse
{
    /// <summary>Unique identifier for the game.</summary>
    public Guid GameId { get; init; }

    /// <summary>Current status of the game.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Number of attempts remaining.</summary>
    public int AttemptsRemaining { get; init; }

    /// <summary>Full history of guesses.</summary>
    public List<GuessDto> History { get; init; } = [];
}