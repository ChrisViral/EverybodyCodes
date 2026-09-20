using System.Text.Json.Serialization;
using Challenge.CLI;
using JetBrains.Annotations;

namespace EverybodyCodes.Resolver;

/// <summary>
/// <see cref="EverybodyCodesResolverSettings"/> JSON source generation context
/// </summary>
[PublicAPI, JsonSerializable(typeof(EverybodyCodesResolverSettings)), JsonSourceGenerationOptions(WriteIndented = true)]
public sealed partial class EverybodyCodesResolverSettingsJsonContext : JsonSerializerContext;

/// <summary>
/// Everybody Codes resolver settings
/// </summary>
/// <param name="Cookie">Request cookie</param>
/// <param name="LastRequestTimestamp">Last request timestamp</param>
/// <param name="Seed">Challenge seed</param>
[PublicAPI]
[method: JsonConstructor]
public sealed record EverybodyCodesResolverSettings(string Cookie, long LastRequestTimestamp, uint? Seed) : ResolverSettings(Cookie, LastRequestTimestamp)
{
    /// <summary>
    /// Challenge seed
    /// </summary>
    public uint? Seed { get; set; } = Seed;
}
