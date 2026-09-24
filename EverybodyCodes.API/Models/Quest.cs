using System.Text.Json.Serialization;
using EverybodyCodes.API.Models.Converters;
using JetBrains.Annotations;

namespace EverybodyCodes.API.Models;

/// <summary>
/// Everybody Codes Quest (challenge) object
/// </summary>
[PublicAPI]
public sealed class Quest
{
    /// <summary>
    /// Until when the user is prevented from submitting an answer
    /// </summary>
    [JsonConverter(typeof(UnixTimeMillisecondsConverter))]
    public DateTimeOffset? PenaltyUntil { get; init; }

    /// <summary>
    /// How much time is left until the user can submit an answer again
    /// </summary>
    [JsonPropertyName("penaltyLeftMs"), JsonConverter(typeof(TimeMillisecondsConverter))]
    public TimeSpan PenaltyLeft { get; init; }

    /// <summary>
    /// Part 1 decryption key
    /// </summary>
    public string? Key1 { get; init; }

    /// <summary>
    /// Part 1 answer
    /// </summary>
    public string? Answer1 { get; init; }

    /// <summary>
    /// Part 2 decryption key
    /// </summary>
    public string? Key2 { get; init; }

    /// <summary>
    /// Part 2 answer
    /// </summary>
    public string? Answer2 { get; init; }

    /// <summary>
    /// Part 3 decryption key
    /// </summary>
    public string? Key3 { get; init; }

    /// <summary>
    /// Part 3 answer
    /// </summary>
    public string? Answer3 { get; init; }

    /// <summary>
    /// Gets the decryption key for a specific part
    /// </summary>
    /// <param name="part">Part to get the key for, must be between 1 and 3 inclusively</param>
    /// <returns>The decryption key for that part</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="part"/> isn't between 1 and 3 inclusively</exception>
    public string? GetKey(uint part) => part switch
    {
        1 => this.Key1,
        2 => this.Key2,
        3 => this.Key3,
        _ => throw new ArgumentOutOfRangeException(nameof(part), "Part must be between 1 and 3 inclusively")
    };

    /// <summary>
    /// Gets the problem answer for a specific part
    /// </summary>
    /// <param name="part">Part to get the key for, must be between 1 and 3 inclusively</param>
    /// <returns>The problem answer for that part</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="part"/> isn't between 1 and 3 inclusively</exception>
    public string? GetAnswer(uint part) => part switch
    {
        1 => this.Answer1,
        2 => this.Answer2,
        3 => this.Answer3,
        _ => throw new ArgumentOutOfRangeException(nameof(part), "Part must be between 1 and 3 inclusively")
    };
}
