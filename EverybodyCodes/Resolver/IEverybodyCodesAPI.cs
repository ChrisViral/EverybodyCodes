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
    [Get("/user/me")]
    Task<User> GetUser(CancellationToken token = default);

    /// <summary>
    /// Gets the input data for a given challenge
    /// </summary>
    /// <param name="url">Input fetch absolute URL</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A dictionary containing the input keyed by part</returns>
    [Get("")]
    Task<Inputs> GetInputs([Url] string url, CancellationToken token = default);

    /// <summary>
    /// Gets the <see cref="Quest"/> API object for a current year and day
    /// </summary>
    /// <param name="year">Quest year</param>
    /// <param name="day">Quest day</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The <see cref="Quest"/> data for the given <paramref name="year"/> and <paramref name="day"/></returns>
    [Get("/event/{year}/quest/{day}")]
    Task<Quest> GetQuest(uint year, uint day, CancellationToken token = default);

    /// <summary>
    /// Posts the answer for the quest of the given year, day, and part
    /// </summary>
    /// <param name="answer">Answer to post</param>
    /// <param name="year">Quest year</param>
    /// <param name="day">Quest day</param>
    /// <param name="part">Quest part</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The API response to the answer</returns>
    [Post("/event/{year}/quest/{day}/part/{part}/answer")]
    Task<AnswerResponse> PostAnswer([Body] AnswerRequest answer, uint year, uint day, uint part, CancellationToken token = default);
}
