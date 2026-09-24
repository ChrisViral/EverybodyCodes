using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace EverybodyCodes.API.Models;

/// <summary>
/// Everybody Codes Inputs API object
/// </summary>
[PublicAPI]
public sealed class Inputs
{
    /// <summary>
    /// First input
    /// </summary>
    [JsonPropertyName("1")]
    public required string Input1 { get; init; }

    /// <summary>
    /// Second input
    /// </summary>
    [JsonPropertyName("2")]
    public required string Input2 { get; init; }

    /// <summary>
    /// Third input
    /// </summary>
    [JsonPropertyName("3")]
    public required string Input3 { get; init; }

    /// <summary>
    /// Gets the input for a specific part
    /// </summary>
    /// <param name="part">Part to get the input for, must be between 1 and 3 inclusively</param>
    /// <returns>The input for that part</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="part"/> isn't between 1 and 3 inclusively</exception>
    public string GetInput(uint part) => part switch
    {
        1 => this.Input1,
        2 => this.Input2,
        3 => this.Input3,
        _ => throw new ArgumentOutOfRangeException(nameof(part), "Part must be between 1 and 3 inclusively")
    };
}
