using Mastermind.Core.Caching;
using Mastermind.Core.Caching.Configuration;
using Mastermind.Core.GamePlay;
using Mastermind.Core.GamePlay.Configuration;
using Mastermind.Core.RandomGeneration;
using Mastermind.Core.RandomGeneration.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace Mastermind.Api.Extensions;

/// <summary>
/// Central place for application dependency injection registrations.
/// Keep framework setup in Program and put app specific wiring here.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services, named HttpClients, etc.
    /// </summary>
    /// <param name="services">Dependency injection service collection.</param>
    /// <param name="configuration">Configuration root.</param>
    /// <param name="environment">Hosting environment.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMastermindServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDistributedMemoryCache();

        // appSettings
        services.Configure<CacheServiceSettings>(configuration.GetSection(CacheServiceSettings.SectionName));
        services.AddOptions<CacheServiceSettings>()
            .Bind(configuration.GetSection(CacheServiceSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.Configure<RandomApiSettings>(
            configuration.GetSection(RandomApiSettings.Name));
        services.AddOptions<RandomApiSettings>()
            .Bind(configuration.GetSection(RandomApiSettings.Name))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.Configure<GamePlaySettings>(
            configuration.GetSection(GamePlaySettings.Name));
        services.AddOptions<GamePlaySettings>()
            .Bind(configuration.GetSection(GamePlaySettings.Name))
            .ValidateDataAnnotations()
            .Validate(s => s.DigitMin <= s.DigitMax,
                "DigitMin must be less than or equal to DigitMax.")
            .Validate(s => s.DefaultAttemptLimit > 0,
                "DefaultAttemptLimit must be greater than zero.")
            .Validate(s => s.SlidingExpirationSeconds is null || s.SlidingExpirationSeconds > 0,
                "SlidingExpirationSeconds must be greater than zero if provided.")
            .Validate(s => s.AbsoluteExpirationSeconds is null || s.AbsoluteExpirationSeconds > 0,
                "AbsoluteExpirationSeconds must be greater than zero if provided.")
            .Validate(s =>
                    s.SlidingExpirationSeconds is null
                    || s.AbsoluteExpirationSeconds is null
                    || s.SlidingExpirationSeconds <= s.AbsoluteExpirationSeconds,
                "SlidingExpirationSeconds must be less than or equal to AbsoluteExpirationSeconds when both are provided.")
            .Validate(s => s.SecretCombinationLength < 4,
                "Invalid GamePlaySettings configuration.")
            .ValidateOnStart();

        // clients
        services.AddHttpClient<IRandomIntegersService, RandomIntegersService>((serviceProvider, http) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<RandomApiSettings>>().Value;
            http.BaseAddress = new Uri(settings.BaseAddress);
            http.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        // services    
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IGamePlayService, GamePlayService>();

        return services;
    }
}