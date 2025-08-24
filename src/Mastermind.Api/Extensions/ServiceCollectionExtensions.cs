using Mastermind.Core.Caching;
using Mastermind.Core.Caching.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mastermind.Api.Extensions
{
	/// <summary>
	/// Central place for application dependency injection registrations.
	/// Keep framework setup in Program and put app specific wiring here.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers application services, named HttpClients, etc..
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
			services.Configure<CacheServiceSettings>(configuration.GetSection(CacheServiceSettings.SectionName));
			services.AddOptions<CacheServiceSettings>()
				.Bind(configuration.GetSection(CacheServiceSettings.SectionName))
				.ValidateDataAnnotations()
				.Validate(opt => opt != null, "CacheService settings are missing.")
				.ValidateOnStart();

			services.AddDistributedMemoryCache();

			services.AddSingleton<ICacheService, CacheService>();

			return services;
		}
	}
}

