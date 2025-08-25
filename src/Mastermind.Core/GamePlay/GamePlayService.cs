using Mastermind.Core.Caching;
using Mastermind.Core.GamePlay.Configuration;
using Mastermind.Core.GamePlay.Models;
using Mastermind.Core.RandomGeneration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mastermind.Core.GamePlay;

/// <inheritdoc />
public class GamePlayService(
    ICacheService cacheService,
    IRandomIntegersService randomIntegersService,
    IOptions<GamePlaySettings> settings) : IGamePlayService
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly IRandomIntegersService _randomIntegersService = randomIntegersService;
    private readonly GamePlaySettings _settings = settings.Value;

    /// <inheritdoc />
    public async Task<GameState> CreateGameAsync(CancellationToken cancellationToken = default)
    {
        var secretCombination =
            await GenerateSecretCombinationAsync(_settings.SecretCombinationLength, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var state = new GameState
        {
            SecretCombination = secretCombination,
            AttemptLimit = _settings.DefaultAttemptLimit,
            AttemptsUsed = 0,
            Status = GameStatus.InProgress,
            CreatedAtUtc = now,
            LastTouchedUtc = now,
            CompletedAtUtc = null
        };
        await PersistGameStateAsync(state.Id.ToString(), state, cancellationToken);

        return state;
    }

    /// <inheritdoc />
    public async Task<GuessEntry> SubmitGuessAsync(Guid gameId, int[] guess,
        CancellationToken cancellationToken = default)
    {
        var state = await GetGameStateOrThrowAsync(gameId, cancellationToken);

        if (state.Status != GameStatus.InProgress)
            throw new InvalidOperationException($"Game {gameId} is already completed with status {state.Status}.");

        // TODO JESSICA: Move this call to ValidateGuessDigits() inside of ComputeFeedback()
        ValidateGuessDigits(guess, state.SecretCombination.Length);
        var feedback = ComputeFeedback(state.SecretCombination, guess);
        // Increment attempts first so AttemptNumber matches AttemptsUsed for this entry
        state.AttemptsUsed++;
        var guessEntry = new GuessEntry
        {
            AttemptNumber = state.AttemptsUsed,
            Guess = guess.ToArray(),
            Feedback = feedback,
            TimestampUtc = DateTimeOffset.UtcNow
        };
        state.GuessHistory.Add(guessEntry);
        if (feedback.CorrectPositions == state.SecretCombination.Length)
        {
            state.Status = GameStatus.Won;
            state.CompletedAtUtc = DateTimeOffset.UtcNow;
        }
        else if (state.AttemptsUsed >= state.AttemptLimit)
        {
            state.Status = GameStatus.Lost;
            state.CompletedAtUtc = DateTimeOffset.UtcNow;
        }

        state.LastTouchedUtc = DateTimeOffset.UtcNow;

        // Persist using the same keying scheme used by your getters
        // If your getters build a key internally from the id, ensure this call does the same
        await PersistGameStateAsync(state.Id.ToString(), state, cancellationToken);

        return guessEntry;
    }

    /// <inheritdoc />
    public async Task<GameState> GetGameAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        return await GetGameStateOrThrowAsync(gameId, cancellationToken);
    }

    #region PRIVATE METHODS

    /// <summary>
    /// Loads the game state from the cache or throws <see cref="KeyNotFoundException" /> if missing.
    /// </summary>
    private async Task<GameState> GetGameStateOrThrowAsync(Guid gameId, CancellationToken cancellationToken)
    {
        var key = gameId.ToString();
        var state = await _cacheService.GetAsync<GameState>(key, cancellationToken);
        if (state is null) throw new KeyNotFoundException($"Game {gameId} was not found.");
        return state;
    }

    /// <summary>
    /// Persists the game state to the cache using configured expirations.
    /// </summary>
    private async Task PersistGameStateAsync(string cacheKey, GameState state, CancellationToken cancellationToken)
    {
        TimeSpan? absoluteExpiration = _settings.AbsoluteExpirationSeconds.HasValue
            ? TimeSpan.FromSeconds(_settings.AbsoluteExpirationSeconds.Value)
            : null;

        TimeSpan? slidingExpiration = _settings.SlidingExpirationSeconds.HasValue
            ? TimeSpan.FromSeconds(_settings.SlidingExpirationSeconds.Value)
            : null;

        await _cacheService.SetAsync(cacheKey, state, absoluteExpiration, slidingExpiration, cancellationToken);
    }

    /// <summary>
    /// Generates the secret combination using the random integers service and requested length.
    /// </summary>
    // TODO JESSICA: Make min and max digits also parameters
    private async Task<int[]> GenerateSecretCombinationAsync(int length, CancellationToken cancellationToken)
    {
        var numbers =
            await _randomIntegersService.GetAsync(length, _settings.DigitMin, _settings.DigitMax, cancellationToken);

        return numbers.ToArray();
    }

    /// <summary>
    /// Validates that the guess has the required length and that each digit is within the configured range.
    /// </summary>
    /// <remarks>
    /// This check enforces the game rules before any game state is updated. It prevents invalid
    /// guesses from entering the history and ensures the user's guess has the same number of digits
    /// as the secretCombination as well as that all digits within the user's guess are within the allowed range.
    /// Validation lives in the service rather than the controller because this places the rules
    /// next to the gameplay logic rather than in HTTP handling code.
    /// </remarks>
    // TODO JESSICA: min and max should be parameters
    private void ValidateGuessDigits(int[] guess, int requiredLength)
    {
        if (guess.Length != requiredLength)
            throw new ArgumentException($"Guess must contain exactly {requiredLength} digits.", nameof(guess));

        foreach (var digit in guess)
            if (digit < _settings.DigitMin || digit > _settings.DigitMax)
                throw new ArgumentOutOfRangeException(nameof(guess),
                    $"Guess digit {digit} is out of range [{_settings.DigitMin}, {_settings.DigitMax}].");
    }

    /// <summary>
    /// Computes feedback for a single guess including counts of correct positions and correct numbers.
    /// </summary>
    private Feedback ComputeFeedback(IList<int> secretCombination, IList<int> guess)
    {
        var correctPositions = CountPositionMatches(secretCombination, guess);

        var countsForSecret = CalculateNumberOccurrences(secretCombination);
        var countsForGuess = CalculateNumberOccurrences(guess);

        var correctNumbers = CountSharedNumbersAcrossTallies(countsForSecret, countsForGuess);

        return new Feedback
        {
            CorrectPositions = correctPositions,
            CorrectNumbers = correctNumbers
        };
    }

    /// <summary>
    /// Counts how many digits match in the same index position between secretCombination and guess.
    /// </summary>
    private int CountPositionMatches(IList<int> secretCombination, IList<int> guess)
    {
        var correctPositions = 0;

        for (var index = 0; index < secretCombination.Count; index++)
        {
            var secretDigit = secretCombination[index];
            var guessDigit = guess[index];

            if (guessDigit == secretDigit) correctPositions++;
        }

        return correctPositions;
    }


    /// <summary>
    /// Calculates how many numbers from the secret appear anywhere in the guess by comparing two tallies
    /// and adding, for each number, the quantity of each number the secret and the guess have in common.
    /// This is the CorrectNumbers total, regardless of position.
    /// </summary>
    /// <remarks>
    /// How this fits in the big picture of assessing a Player's guess:
    /// Step 1 counts same position matches to get CorrectPositions.
    /// Step 2 builds a tally of how many times each number appears in the secret and another tally for the guess.
    /// Step 3 is this method which counts what the secret and the guess have in common.
    /// </remarks>
    /// <example>
    /// Secret: 1 1 2 3
    /// Guess:  1 2 2 4
    /// Secret tallies: {1:2, 2:1, 3:1}
    /// Guess tallies:  {1:1, 2:2, 4:1}
    /// Shared amounts per number: 1 contributes 1, 2 contributes 1, 3 contributes 0, 4 contributes 0.
    /// CorrectNumbers = 1 + 1 + 0 + 0 = 2.
    /// </example>
    /// <returns>
    /// The total count of correct numbers anywhere in the guess.
    /// </returns>
    private int CountSharedNumbersAcrossTallies(
        Dictionary<int, int> secretCombinationNumbersTally,
        Dictionary<int, int> guessNumbersTally)
    {
        var totalCorrectNumbers = 0;

        foreach (var entry in secretCombinationNumbersTally)
        {
            var number = entry.Key;
            var countInSecretCombination = entry.Value;
            var countInGuess = guessNumbersTally.GetValueOrDefault(number, 0);
            totalCorrectNumbers += Math.Min(countInSecretCombination, countInGuess);
        }

        return totalCorrectNumbers;
    }

    /// <summary>
    /// Counts how many times each number appears in the input sequence.
    /// The caller <see cref="ComputeFeedback" /> uses these counts to calculate the total of correct numbers
    /// anywhere in the guess, without caring about positions.
    /// </summary>
    /// <remarks>
    /// How this fits in the big picture of assessing a Player's guess:
    /// The caller uses this method to build a tally for the secretCombination and another tally for the guess.
    /// Then in the caller, for each number present in either the secretCombination or the guess,
    /// we count it only as many as the least number of times it appears in one or the other of the two tallies.
    /// </remarks>
    /// <example>
    /// Example: secret 1 1 2 3 and guess 1 2 2 4 share one 1 and one 2, so CorrectNumbers is 2.
    /// </example>
    /// <param name="numberSequence"> The sequence of numbers to tally.</param>
    /// <returns>
    /// A dictionary where each key is a number from the input and the value is how many times that
    /// number appears in the input sequence.
    /// </returns>
    private Dictionary<int, int> CalculateNumberOccurrences(IEnumerable<int> numberSequence)
    {
        // Dictionary for tracking how many times a number occurs in the digit sequence.
        var numberOccurrences = new Dictionary<int, int>();

        foreach (var number in numberSequence)
        {
            // Retrieve the current count for this number.
            // If the number is not yet in the dictionary, set the count to zero.
            var existingCount = numberOccurrences.GetValueOrDefault(number, 0);
            numberOccurrences[number] = existingCount + 1;
        }

        return numberOccurrences;
    }

    #endregion PRIVATE METHODS
}