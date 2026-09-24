using System.Text.Json.Serialization;
using Challenge.CLI;
using JetBrains.Annotations;

namespace EverybodyCodes;

/// <summary>
/// <see cref="EverybodyCodesSettings"/> JSON source generation context
/// </summary>
[PublicAPI, JsonSerializable(typeof(EverybodyCodesSettings)), JsonSourceGenerationOptions(WriteIndented = true)]
public sealed partial class EverybodyCodesSettingsJsonContext : JsonSerializerContext;

/// <summary>
/// Everybody Codes resolver settings
/// </summary>
/// <param name="Cookie">Request cookie</param>
/// <param name="LastRequestTimestamp">Last request timestamp</param>
/// <param name="Seed">Challenge seed</param>
[PublicAPI]
[method: JsonConstructor]
public sealed record EverybodyCodesSettings(string Cookie, long LastRequestTimestamp, uint? Seed) : ResolverSettings(Cookie, LastRequestTimestamp)
{
    /// <summary>
    /// Challenge seed
    /// </summary>
    public uint? Seed { get; set; } = Seed;
}
