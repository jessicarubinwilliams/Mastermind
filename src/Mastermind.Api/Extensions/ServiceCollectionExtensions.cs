namespace Mastermind.Api.Extensions;

/// <summary>
/// Central place for application dependency injection registrations.
/// Keep framework setup in Program and put app specific wiring here.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services, named HttpClients, etc..
    /// </summary>
    public static IServiceCollection AddMastermindServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        return services;
    }
}
