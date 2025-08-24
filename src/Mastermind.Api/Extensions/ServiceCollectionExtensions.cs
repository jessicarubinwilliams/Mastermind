using Mastermind.Core.Caching;
using Mastermind.Core.Caching.Configuration;
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

        // clients
        services.AddHttpClient<IRandomIntegersService, RandomIntegersService>((serviceProvider, http) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<RandomApiSettings>>().Value;
            http.BaseAddress = new Uri(settings.BaseAddress);
            http.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        // service    
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}