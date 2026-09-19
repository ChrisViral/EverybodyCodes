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
[PublicAPI]
[method: JsonConstructor]
public sealed record EverybodyCodesResolverSettings(string Cookie, long LastRequestTimestamp, int? Seed) : ResolverSettings(Cookie, LastRequestTimestamp);
