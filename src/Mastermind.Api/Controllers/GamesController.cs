using Mastermind.Api.Models.GamePlay;
using Mastermind.Core.GamePlay;
using Mastermind.Core.GamePlay.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mastermind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GamesController(IGamePlayService gamePlayService, ILogger<GamesController> logger) : ControllerBase
{
    private readonly IGamePlayService _gamePlayService = gamePlayService;
    private readonly ILogger<GamesController> _logger = logger;

    /// <summary>
    /// Create a new game and return initial state.
    /// </summary>
    /// <returns>GameResponse with initial status and attempts remaining.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var state = await _gamePlayService.CreateGameAsync(cancellationToken);
            var response = MapToGameResponse(state);
            return CreatedAtRoute("GetGame", new { id = response.GameId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid request creating game");
            return BadRequest(new ProblemDetails { Title = "Invalid request", Detail = ex.Message });
        }
    }

    /// <summary>
    /// Submit a guess for a game and return the updated game state.
    /// </summary>
    /// <param name="id">Game identifier.</param>
    /// <param name="request">Guess payload.</param>
    /// <returns>GameResponse with updated status, attempts remaining, and full history.</returns>
    [HttpPost("{id:guid}/guesses")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitGuessAsync([FromRoute] Guid id, [FromBody] GuessRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _gamePlayService.SubmitGuessAsync(id, request.Guess, cancellationToken);

            var state = await _gamePlayService.GetGameAsync(id, cancellationToken);
            var response = MapToGameResponse(state);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundProblem(ex, "Game not found for id {GameId}", id);
        }
        catch (InvalidOperationException ex)
        {
            return ConflictProblem(ex, "Conflict submitting guess for game id {GameId}", id);
        }
        catch (ArgumentException ex)
        {
            return BadRequestProblem(ex, "Bad request submitting guess for game id {GameId}", id);
        }
    }

    /// <summary>
    /// Get the current state of a game, including history.
    /// </summary>
    /// <param name="id">Game identifier.</param>
    /// <returns>GameResponse with status, attempts remaining, and full history.</returns>
    [HttpGet("{id:guid}", Name = "GetGame")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var state = await _gamePlayService.GetGameAsync(id, cancellationToken);
            var response = MapToGameResponse(state);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundProblem(ex, "Game not found for id {GameId}", id);
        }
    }

    #region PRIVATE METHODS

    /// <summary>
    /// Maps a <see cref="Mastermind.Core.GamePlay.Models.GameState" /> to a <see cref="GameResponse" />.
    /// </summary>
    /// <param name="state">The <see cref="Mastermind.Core.GamePlay.Models.GameState" /> returned by the service.</param>
    /// <returns>A populated <see cref="GameResponse" /> for API consumers.</returns>
    private GameResponse MapToGameResponse(GameState state)
    {
        return new GameResponse
        {
            GameId = state.Id,
            Status = state.Status.ToString(),
            AttemptsRemaining = state.AttemptLimit - state.AttemptsUsed,
            History = MapHistory(state.GuessHistory)
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="Mastermind.Core.GamePlay.Models.GuessEntry" /> to a list of <see cref="GuessDto" />.
    /// </summary>
    /// <param name="history">
    /// The <see cref="System.Collections.Generic.IEnumerable{T}" /> of
    /// <see cref="Mastermind.Core.GamePlay.Models.GuessEntry" />.
    /// </param>
    /// <returns>A list of <see cref="GuessDto" /> suitable for API responses.</returns>
    private List<GuessDto> MapHistory(IEnumerable<GuessEntry> history)
    {
        return history
            .Select(MapGuessEntry)
            .ToList();
    }

    /// <summary>
    /// Maps a single <see cref="Mastermind.Core.GamePlay.Models.GuessEntry" /> to a <see cref="GuessDto" />.
    /// </summary>
    /// <param name="entry">The <see cref="Mastermind.Core.GamePlay.Models.GuessEntry" /> to map.</param>
    /// <returns>A <see cref="GuessDto" /> with feedback and metadata.</returns>
    private GuessDto MapGuessEntry(GuessEntry entry)
    {
        return new GuessDto
        {
            Attempt = entry.AttemptNumber,
            Guess = entry.Guess,
            CorrectNumbers = entry.Feedback.CorrectNumbers,
            CorrectPositions = entry.Feedback.CorrectPositions,
            AtUtc = entry.TimestampUtc
        };
    }

    /// <summary>
    /// Logs a not found error and returns a standardized ProblemDetails 404 response.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="messageTemplate">The log message template.</param>
    /// <param name="gameId">The game identifier for context.</param>
    /// <returns>NotFound result with ProblemDetails payload.</returns>
    private IActionResult NotFoundProblem(Exception ex, string messageTemplate, Guid gameId)
    {
        _logger.LogError(ex, messageTemplate, gameId);
        return NotFound(new ProblemDetails { Title = "Game not found", Detail = ex.Message });
    }

    /// <summary>
    /// Logs a conflict error and returns a standardized ProblemDetails 409 response.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="messageTemplate">The log message template.</param>
    /// <param name="gameId">The game identifier for context.</param>
    /// <returns>Conflict result with ProblemDetails payload.</returns>
    private IActionResult ConflictProblem(Exception ex, string messageTemplate, Guid gameId)
    {
        _logger.LogError(ex, messageTemplate, gameId);
        return Conflict(new ProblemDetails { Title = "Conflict submitting guess", Detail = ex.Message });
    }

    /// <summary>
    /// Logs a bad request error and returns a standardized ProblemDetails 400 response.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="messageTemplate">The log message template.</param>
    /// <param name="gameId">The game identifier for context.</param>
    /// <returns>BadRequest result with ProblemDetails payload.</returns>
    private IActionResult BadRequestProblem(Exception ex, string messageTemplate, Guid gameId)
    {
        _logger.LogError(ex, messageTemplate, gameId);
        return BadRequest(new ProblemDetails { Title = "Invalid request", Detail = ex.Message });
    }

    #endregion
}