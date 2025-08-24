using Mastermind.Core.RandomGeneration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mastermind.Core.RandomGeneration;

/// <summary>
/// Calls random dot org to fetch integers. Falls back to local generation on any failure.
/// </summary>
public class RandomIntegersService(
    HttpClient httpClient,
    IOptions<RandomApiSettings> settings,
    ILogger<RandomIntegersService> logger) : IRandomIntegersService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<RandomIntegersService> _logger = logger;
    private readonly RandomApiSettings _settings = settings.Value;

    /// <inheritdoc />
    public async Task<int[]> GetAsync(int count = 4, int min = 0, int max = 7,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var uri =
                $"{_settings.IntegersEndpoint}?num={count}&min={min}&max={max}&col=1&base=10&format=plain&rnd=new";
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Split the plain-text response (one integer per line) into separate string values.
            // Random.org returns each integer on its own line,
            // so we split on carriage return and line feed.
            // RemoveEmptyEntries ensures blank lines are ignored.
            var rawNumbers = responseContent.Split((char[])['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);


            if (rawNumbers.Length != count)
                return LocalRandomIntegerGeneration(count, min, max);

            // Parse lines directly to integers
            var integers = Array.ConvertAll(rawNumbers, int.Parse);

            return integers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request to random.org failed. Using local fallback.");
            return LocalRandomIntegerGeneration(count, min, max);
        }
    }

    /// <summary>
    /// Generates a set of integers locally using Random.Shared when the remote service is unavailable
    /// or returns invalid data. Values are in the inclusive range specified by min and max.
    /// </summary>
    /// <param name="count">Number of integers to generate.</param>
    /// <param name="min">Inclusive minimum value.</param>
    /// <param name="max">Inclusive maximum value.</param>
    /// <returns>An array of locally generated integers.</returns>
    private static int[] LocalRandomIntegerGeneration(int count, int min, int max)
    {
        return Enumerable.Range(0, count)
            .Select(_ => Random.Shared.Next(min, max + 1))
            .ToArray();
    }
}