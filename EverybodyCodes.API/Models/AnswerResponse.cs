using System.Text.Json.Serialization;
using EverybodyCodes.API.Models.Converters;
using JetBrains.Annotations;

namespace EverybodyCodes.API.Models;

/// <summary>
/// Everybody Codes Answer API response
/// </summary>
[PublicAPI]
public sealed class AnswerResponse
{
    /// <summary>
    /// If the answer is correct
    /// </summary>
    public bool Correct { get; init; }

    /// <summary>
    /// If the answer is of the correct length
    /// </summary>
    public bool LengthCorrect { get; init; }

    /// <summary>
    /// If the first character of the answer is correct
    /// </summary>
    public bool FirstCorrect { get; init; }

    /// <summary>
    /// When can the user submit an answer again
    /// </summary>
    [JsonConverter(typeof(UnixTimeMillisecondsConverter))]
    public DateTimeOffset PenaltyUntil { get; init; }

    /// <summary>
    /// How long until the user can submit an answer again
    /// </summary>
    [JsonPropertyName("penaltyLeftMs"), JsonConverter(typeof(TimeMillisecondsConverter))]
    public TimeSpan PenaltyLeft { get; init; }
}
