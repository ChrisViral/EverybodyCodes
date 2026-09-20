using System.Text.Json.Serialization;
using EverybodyCodes.Resolver.Models.Converters;
using JetBrains.Annotations;

namespace EverybodyCodes.Resolver.Models;

/// <summary>
/// Everybody Codes User API object
/// </summary>
[PublicAPI]
public class User
{
    /// <summary>
    /// User ID
    /// </summary>
    [JsonPropertyName("id")]
    public ulong ID { get; init; }

    /// <summary>
    /// User security code
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Username
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// If this user is an admin
    /// </summary>
    [JsonConverter(typeof(NumericalBoolConverter))]
    public bool Admin { get; init; }

    /// <summary>
    /// User level
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Events solve count
    /// </summary>
    public int Events { get; init; }

    /// <summary>
    /// Stories solve count
    /// </summary>
    public int Stories { get; init; }

    /// <summary>
    /// GridOS solve count
    /// </summary>
    [JsonPropertyName("gridos")]
    public int GridOS { get; init; }

    /// <summary>
    /// If GrisOS solutions are public
    /// </summary>
    [JsonPropertyName("gridosSolutions")]
    public string? GridOSSolutions { get; init; }

    /// <summary>
    /// Repository URL
    /// </summary>
    [JsonConverter(typeof(EmptyUriConverter))]
    public Uri? Url { get; init; }

    /// <summary>
    /// Country code
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Main programming language
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Challenges seed
    /// </summary>
    public int Seed { get; init; }

    /// <summary>
    /// Steam URL
    /// </summary>
    [JsonConverter(typeof(EmptyUriConverter))]
    public Uri? Stream { get; init; }

    /// <summary>
    /// If this is a confirmed streamer
    /// </summary>
    [JsonConverter(typeof(NumericalBoolConverter))]
    public bool Streamer { get; init; }

    /// <summary>
    /// If this account uses AI for solves
    /// </summary>
    [JsonPropertyName("ai"), JsonConverter(typeof(NumericalBoolConverter))]
    public bool AI { get; init; }

    /// <summary>
    /// User custom message
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Earned user badges
    /// </summary>
    public Dictionary<int, object?>? Badges { get; init; }

    /// <summary>
    /// Last update server time
    /// </summary>
    [JsonConverter(typeof(UnixTimeMillisecondsConverter))]
    public DateTimeOffset ServerTime { get; init; }

    /// <summary>
    /// Current session key
    /// </summary>
    public required string SessionToken { get; init; }
}
