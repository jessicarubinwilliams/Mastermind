using System.Threading;
using System.Threading.Tasks;

namespace Mastermind.Core.RandomGeneration;

/// <summary>
/// Retrieves random integers from an external source with a local fallback.
/// </summary>
public interface IRandomIntegersService
{
    /// <summary>
    /// Requests a sequence of random integers within the specified inclusive range.
    /// </summary>
    /// <param name="count">The number of integers to return.</param>
    /// <param name="min">The inclusive minimum value of the range.</param>
    /// <param name="max">The inclusive maximum value of the range.</param>
    /// <param name="cancellationToken">
    /// Propagates request-abort notifications. This ensures that if the caller (such as an ASP.NET Core controller)
    /// is cancelled because the client disconnected or the HTTP request was aborted, the outbound call to the
    /// external API is also cancelled instead of wasting resources.
    /// </param>
    /// <returns>An array of random integers in the requested range.</returns>
    Task<int[]> GetAsync(int count = 4, int min = 0, int max = 7, CancellationToken cancellationToken = default);
}