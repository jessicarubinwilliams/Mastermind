using System.ComponentModel.DataAnnotations;

namespace Mastermind.Core.Caching.Configuration
{
    /// <summary>
    /// Configuration bound from the "CacheService" section in appsettings.
    /// Controls diagnostics and default expirations applied by the cache service.
    /// </summary>
    public class CacheServiceSettings
    {
        /// <summary>
        /// Name of the configuration section for binding.
        /// </summary>
        public const string SectionName = "CacheService";

		/// <summary>
		/// When true, the cache service writes Debug logs whenever a requested item is not found in the cache 
		/// (a cache miss) and whenever a new item is set in the cache. 
		/// Errors are always logged regardless of this setting.
		/// </summary>
        public bool EnableDiagnostics { get; set; } = false;

		/// <summary>
		/// Default absolute expiration in seconds.
		/// </summary>
		/// <remarks>
		/// A value of zero disables absolute expiration by default, meaning cached items will not
		/// automatically expire at a fixed point in time unless explicitly specified by the caller.
		/// This design avoids evicting items that are still actively used and instead relies on
		/// sliding expiration for general scenarios. Absolute expiration should be enabled only
		/// when a strict cutoff time is required, such as expiring security tokens.
		/// </remarks>
        [Range(0, int.MaxValue)]
        public int DefaultAbsoluteExpirationSeconds { get; set; } = 0;

        /// <summary>
        /// Default sliding expiration in seconds.
        /// </summary>
		/// <remarks>
		/// The default value is 1800 seconds (30 minutes). This ensures that actively used
		/// cache entries remain available, while entries that go unused are naturally evicted
		/// after a reasonable period.
		/// </remarks>
        [Range(0, int.MaxValue)]
        public int DefaultSlidingExpirationSeconds { get; set; } = 1800;
    }
}
