using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mastermind.Core.Caching
{
    /// <summary>
    /// Minimal cache operations on top of IDistributedCache.
    /// Callers can provide a full key or a set of key segments.
    /// The implementation is responsible for normalizing keys.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value by key or, on a miss, creates it with the supplied factory and stores it.
        /// </summary>
        /// <typeparam name="T">
        /// Reference type to allow returning null on a cache miss prior to creation.
        /// </typeparam>
        /// <param name="key">Arbitrary key text. The service will normalize it.</param>
        /// <param name="factory">Asynchronous factory that produces the value when the key is not present.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration relative to now. When null or zero, defaults may be applied.</param>
        /// <param name="slidingExpiration">Optional sliding expiration. When null or zero, defaults may be applied.</param>
        /// <param name="cancellationToken">Cancellation for both cache operations and the factory.</param>
        /// <returns>The cached or newly created value.</returns>
        /// <remarks>
        /// Use this when you already have a complete key string.
        /// </remarks>
        Task<T?> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Gets a value by composed key segments or, on a miss, creates it with the supplied factory and stores it.
        /// </summary>
        /// <typeparam name="T">
        /// Reference type to allow returning null on a cache miss prior to creation.
        /// </typeparam>
        /// <param name="factory">Asynchronous factory that produces the value when the key is not present.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration relative to now. When null or zero, defaults may be applied.</param>
        /// <param name="slidingExpiration">Optional sliding expiration. When null or zero, defaults may be applied.</param>
        /// <param name="cancellationToken">Cancellation for both cache operations and the factory.</param>
        /// <param name="keySegments">Ordered fragments such as app, environment, tenant, feature, and identifier. The service will compose and normalize them.</param>
        /// <returns>The cached or newly created value.</returns>
        /// <remarks>
        /// Use this when the key is naturally built from parts. The service will join and normalize the parts.
        /// </remarks>
        Task<T?> GetOrCreateAsync<T>(
            Func<CancellationToken, Task<T?>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default,
            params string[] keySegments) where T : class;

        /// <summary>
        /// Gets a value from the cache or returns null when not present.
        /// </summary>
        /// <typeparam name="T">Reference type of the stored value.</typeparam>
        /// <param name="key">Arbitrary key text. The service will normalize it.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The cached value or null.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Gets a value using composed key segments or returns null when not present.
        /// </summary>
        /// <typeparam name="T">Reference type of the stored value.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="keySegments">Ordered fragments such as app, environment, tenant, feature, and identifier. The service will compose and normalize them.</param>
        /// <returns>The cached value or null.</returns>
        Task<T?> GetAsync<T>(CancellationToken cancellationToken = default, params string[] keySegments) where T : class;

        /// <summary>
        /// Stores the value under the given key, replacing any existing entry.
        /// </summary>
        /// <typeparam name="T">Serializable type of the value. Value types and reference types are both supported.</typeparam>
        /// <param name="key">Arbitrary key text. The service will normalize it.</param>
        /// <param name="value">Value to store. Null values are not cached.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration relative to now.</param>
        /// <param name="slidingExpiration">Optional sliding expiration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores the value under a composed key, replacing any existing entry.
        /// </summary>
        /// <typeparam name="T">Serializable type of the value. Value types and reference types are both supported.</typeparam>
        /// <param name="value">Value to store. Null values are not cached.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration relative to now.</param>
        /// <param name="slidingExpiration">Optional sliding expiration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="keySegments">Ordered fragments such as app, environment, tenant, feature, and identifier. The service will compose and normalize them.</param>
        Task SetAsync<T>(
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default,
            params string[] keySegments);

        /// <summary>
        /// Removes the cache entry for the key when it exists.
        /// </summary>
        /// <param name="key">Arbitrary key text. The service will normalize it.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the cache entry for a composed key when it exists.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="keySegments">Ordered fragments such as app, environment, tenant, feature, and identifier. The service will compose and normalize them.</param>
        Task RemoveAsync(CancellationToken cancellationToken = default, params string[] keySegments);
    }
}
