namespace Mastermind.Core.GamePlay.Configuration;

public sealed class GamePlaySettings
{
    /// <summary>
    /// Name of the configuration section in appSettings.
    /// </summary>
    public const string Name = "GamePlay";

    /// <summary>
    /// Maximum number of guesses allowed per game.
    /// </summary>
    public int DefaultAttemptLimit { get; init; } = 10;

    /// <summary>
    /// Minimum digit value allowed in secrets and guesses.
    /// </summary>
    public int DigitMin { get; init; }

    /// <summary>
    /// Maximum digit value allowed in secrets and guesses.
    /// </summary>
    public int DigitMax { get; init; } = 7;

    /// <summary>
    /// Number of digits in the secret combination and in each guess.
    /// </summary>
    public int SecretCombinationLength { get; init; } = 4;

    /// <summary>
    /// Optional sliding expiration in seconds. If null, sliding expiration is not applied.
    /// </summary>
    public int? SlidingExpirationSeconds { get; init; }

    /// <summary>
    /// Optional absolute expiration in seconds. If null, absolute expiration is not applied.
    /// </summary>
    public int? AbsoluteExpirationSeconds { get; init; }
}