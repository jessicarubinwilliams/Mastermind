namespace Mastermind.Core.RandomGeneration.Configuration;

/// <summary>
/// Settings for the random integers HTTP client.
/// </summary>
public class RandomApiSettings
{
    /// <summary>
    /// Section name in configuration.
    /// </summary>
    public const string Name = "RandomApi";

    /// <summary>
    /// Base URI for the random service.
    /// </summary>
    public string BaseAddress { get; init; } = "https://www.random.org/";

    /// <summary>
    /// Relative endpoint for random integers.
    /// </summary>
    public string IntegersEndpoint { get; init; } = "integers/";

    /// <summary>
    /// Timeout in seconds for outbound calls.
    /// </summary>
    /// <remarks>
    /// Ensures game play not impacted by hanging API calls
    /// </remarks>
    public int TimeoutSeconds { get; init; } = 5;
}