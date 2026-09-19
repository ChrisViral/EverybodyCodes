using EverybodyCodes.Resolver.Models;
using Refit;

namespace EverybodyCodes.Resolver;

/// <summary>
/// Everybody Codes API
/// </summary>
public interface IEverybodyCodesAPI
{
    /// <summary>
    /// Gets the <see cref="User"/> API object
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>The authenticated user data</returns>
    [Get("/api/user/me")]
    Task<User> GetUser(CancellationToken token = default);
}
