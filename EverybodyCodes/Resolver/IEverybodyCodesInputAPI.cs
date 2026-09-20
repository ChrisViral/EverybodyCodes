using EverybodyCodes.Resolver.Models;
using Refit;

namespace EverybodyCodes.Resolver;

/// <summary>
/// Everybody Codes input API
/// </summary>
public interface IEverybodyCodesInputAPI
{
    /// <summary>
    /// Gets the <see cref="Inputs"/> API object for a current year, say, and seed
    /// </summary>
    /// <param name="year">Quest year</param>
    /// <param name="day">Quest day</param>
    /// <param name="seed">User seed</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The <see cref="Inputs"/> data for the given <paramref name="year"/>, <paramref name="day"/>, and <paramref name="seed"/></returns>
    [Get("/assets/{year}/{day}/input/{seed}.json")]
    Task<Inputs> GetInputs(uint year, uint day, uint seed, CancellationToken token = default);
}
