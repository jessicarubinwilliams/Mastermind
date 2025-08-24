using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mastermind.Core.Caching.Configuration;

namespace Mastermind.Core.Caching
{
	/// <summary>
	/// Provides a simple wrapper around the distributed cache.
	/// Handles storing, retrieving, and removing values with optional expirations.
	/// Can log diagnostic information if enabled in settings.
	/// </summary>
	public class CacheService(
		IDistributedCache cache,
		ILogger<CacheService> logger,
		IOptions<CacheServiceSettings> settings) : ICacheService
	{
		private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
		{
			WriteIndented = false
		};

		private readonly IDistributedCache _cache = cache;
		private readonly ILogger<CacheService> _logger = logger;
		private readonly CacheServiceSettings _settings = settings.Value;

		/// <inheritdoc />
		public async Task<T?> GetOrCreateAsync<T>(
			string key,
			Func<CancellationToken, Task<T?>> factory,
			TimeSpan? absoluteExpiration = null,
			TimeSpan? slidingExpiration = null,
			CancellationToken cancellationToken = default) where T : class
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key must be provided.", nameof(key));

			if (factory is null)
				throw new ArgumentNullException(nameof(factory));

			var normalizedKey = NormalizeKey(key);
			var cached = await GetAsync<T>(normalizedKey, cancellationToken).ConfigureAwait(false);
			
			if (cached is not null)
				return cached;

			if (_settings.EnableDiagnostics)
				_logger.LogDebug("Cache miss for key {Key}", normalizedKey);

			T? created;
			
			try
			{
				created = await factory(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cache factory failed for key {Key}", normalizedKey);
				throw;
			}

			// do not cache nulls
			if (created is null)
				return null;

			await SetAsync(normalizedKey, created, absoluteExpiration, slidingExpiration, cancellationToken)
				.ConfigureAwait(false);

			return created;
		}

		/// <inheritdoc />
		public Task<T?> GetOrCreateAsync<T>(
			Func<CancellationToken, Task<T?>> factory,
			TimeSpan? absoluteExpiration = null,
			TimeSpan? slidingExpiration = null,
			CancellationToken cancellationToken = default,
			params string[] keySegments) where T : class
		{
			var normalizedKey = BuildKey(keySegments);
			return GetOrCreateAsync(normalizedKey, factory, absoluteExpiration, slidingExpiration, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key must be provided.", nameof(key));

			var normalizedKey = NormalizeKey(key);

			try
			{
				var bytes = await _cache.GetAsync(normalizedKey, cancellationToken).ConfigureAwait(false);

				if (bytes is null || bytes.Length == 0)
					return null;

				return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cache get failed for key {Key}", normalizedKey);
				throw;
			}
		}

		/// <inheritdoc />
		public Task<T?> GetAsync<T>(CancellationToken cancellationToken = default, params string[] keySegments) where T : class
		{
			var normalizedKey = BuildKey(keySegments);
			return GetAsync<T>(normalizedKey, cancellationToken);
		}

		/// <inheritdoc />
		public async Task SetAsync<T>(
			string key,
			T value,
			TimeSpan? absoluteExpiration = null,
			TimeSpan? slidingExpiration = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key must be provided.", nameof(key));

			var normalizedKey = NormalizeKey(key);

			try
			{
				var entryOptions = BuildOptions(absoluteExpiration, slidingExpiration);
				// DistributedCache requires a byte array.
				// SerializeToUtf8Bytes avoids creating an intermediate JSON string and then re-encoding it to UTF-8.
				var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
				await _cache.SetAsync(normalizedKey, bytes, entryOptions, cancellationToken).ConfigureAwait(false);

				if (_settings.EnableDiagnostics)
					_logger.LogDebug("Cache set for key {Key}", normalizedKey);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cache set failed for key {Key}", normalizedKey);
				throw;
			}
		}

		/// <inheritdoc />
		public Task SetAsync<T>(
			T value,
			TimeSpan? absoluteExpiration = null,
			TimeSpan? slidingExpiration = null,
			CancellationToken cancellationToken = default,
			params string[] keySegments)
		{
			var normalizedKey = BuildKey(keySegments);
			return SetAsync(normalizedKey, value, absoluteExpiration, slidingExpiration, cancellationToken);
		}

		/// <inheritdoc />
		public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key must be provided.", nameof(key));

			var normalizedKey = NormalizeKey(key);

			try
			{
				await _cache.RemoveAsync(normalizedKey, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cache remove failed for key {Key}", normalizedKey);
				throw;
			}
		}

		/// <inheritdoc />
		public Task RemoveAsync(CancellationToken cancellationToken = default, params string[] keySegments)
		{
			var normalizedKey = BuildKey(keySegments);
			return RemoveAsync(normalizedKey, cancellationToken);
		}

		/// <summary>
		/// Builds a <see cref="DistributedCacheEntryOptions"/> object by applying either the caller's
		/// provided expiration values or falling back to defaults from configuration. This object is
		/// required by the <see cref="IDistributedCache"/> API to specify cache entry lifetimes.
		/// </summary>
		/// <param name="absoluteExpiration">
		/// Absolute expiration relative to now. When null or zero, the default value from
		/// <see cref="CacheServiceSettings.DefaultAbsoluteExpirationSeconds"/> is applied (if greater than zero).
		/// </param>
		/// <param name="slidingExpiration">
		/// Sliding expiration. When null or zero, the default value from
		/// <see cref="CacheServiceSettings.DefaultSlidingExpirationSeconds"/> is applied (if greater than zero).
		/// </param>
		/// <returns>
		/// A configured <see cref="DistributedCacheEntryOptions"/> instance containing the effective
		/// expiration policy for the cache entry.
		/// </returns>
		private DistributedCacheEntryOptions BuildOptions(TimeSpan? absoluteExpiration, TimeSpan? slidingExpiration)
		{
			var options = new DistributedCacheEntryOptions();

			// If valid absolute expiration was provided
			if (absoluteExpiration is not null && absoluteExpiration.Value > TimeSpan.Zero)
				options.SetAbsoluteExpiration(absoluteExpiration.Value);
			// If not but a default is configured
			else if (_settings.DefaultAbsoluteExpirationSeconds > 0)
				options.SetAbsoluteExpiration(TimeSpan.FromSeconds(_settings.DefaultAbsoluteExpirationSeconds));

			// If valid sliding expiration was provided
			if (slidingExpiration is not null && slidingExpiration.Value > TimeSpan.Zero)
				options.SetSlidingExpiration(slidingExpiration.Value);
			// If not but a default is configured
			else if (_settings.DefaultSlidingExpirationSeconds > 0)
				options.SetSlidingExpiration(TimeSpan.FromSeconds(_settings.DefaultSlidingExpirationSeconds));

			return options;
		}


		/// <summary>
		/// Composes and normalizes a cache key from ordered segments.
		/// </summary>
		/// <param name="segments">Fragments such as app, env, tenant, feature, id. Null or empty segments are ignored.</param>
		/// <returns>A normalized key ready for the cache backend.</returns>
		private string BuildKey(params string[] segments)
		{
			if (segments is null || segments.Length == 0)
				throw new ArgumentException("At least one key segment must be provided.", nameof(segments));

			var cleaned = segments
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(NormalizeSegment)
				.Where(s => s.Length > 0);

			var joined = string.Join(":", cleaned);

			return NormalizeKey(joined);
		}

		/// <summary>
		/// Normalizes a complete key string.
		/// </summary>
		/// <remarks>
		/// Trim, convert to lower invariant, collapse internal whitespace to a single colon,
		/// strip control characters, and cap length to a reasonable maximum.
		/// Idempotent by design.
		/// </remarks>
		private string NormalizeKey(string key)
		{
			var trimmed = key.Trim().ToLowerInvariant();

			// Replace any run of whitespace or colons with a single colon
			trimmed = Regex.Replace(trimmed, @"[\s:]+", ":", RegexOptions.CultureInvariant);

			// Remove non-printable control characters
			var sb = new StringBuilder(trimmed.Length);
			foreach (var ch in trimmed)
			{
				if (!char.IsControl(ch))
					sb.Append(ch);
			}

			var result = sb.ToString();

			// Optional defensive cap to avoid extremely long keys
			const int maxLen = 256;
			if (result.Length > maxLen)
				result = result.Substring(0, maxLen);

			return result;
		}

		/// <summary>
		/// Normalizes an individual key segment by trimming, lowercasing,
		/// collapsing consecutive whitespace, and removing control characters.
		/// Spaces are replaced with a safe separator character.
		/// </summary>
		private static string NormalizeSegment(string segment)
		{
			if (string.IsNullOrWhiteSpace(segment))
				return string.Empty;

			var s = segment.Trim().ToLowerInvariant();

			// Collapse any run of whitespace into a single space
			s = Regex.Replace(s, @"\s+", " ", RegexOptions.CultureInvariant);

			var sb = new StringBuilder(s.Length);

			// Remove control characters (non-printable chars like tab, newline, null, etc.)
			// to ensure cache keys are always clean and safe for storage/logging
			foreach (var ch in s)
			{
				if (!char.IsControl(ch))
					sb.Append(ch);
			}

			// Replace spaces with colons
			return sb.ToString().Replace(' ', ':');
		}
	}
}
