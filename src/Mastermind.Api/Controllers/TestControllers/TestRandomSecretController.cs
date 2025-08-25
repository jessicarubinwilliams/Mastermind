using Mastermind.Core.RandomGeneration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Mastermind.Api.Controllers.TestControllers;

/// <summary>
/// Test controller for verifying random secret code generation via the integers service.
/// Returns four integers in the inclusive range zero through seven.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class TestRandomSecretController(IRandomIntegersService randomIntegers) : ControllerBase
{
    private readonly IRandomIntegersService _randomIntegers = randomIntegers;

    /// <summary>
    /// Generates and returns a new four number secret code.
    /// </summary>
    /// <remarks>
    /// Uses the request abort token so downstream work stops immediately if the client disconnects.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(int[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int[]>> GetAsync()
    {
        // HttpContext.RequestAborted is cancelled if the client disconnects or the server aborts the request.
        var ct = HttpContext.RequestAborted;

        var code = await _randomIntegers.GetAsync(cancellationToken: ct);
        return Ok(code);
    }
}