using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Challenge.CLI;
using JetBrains.Annotations;

namespace EverybodyCodes.Resolver;

/// <summary>
/// <see cref="EverybodyCodesSettings"/> JSON source generation context
/// </summary>
[PublicAPI, JsonSerializable(typeof(EverybodyCodesSettings)), JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class EverybodyCodesSettingsJsonContext : JsonSerializerContext;

/// <summary>
/// Everybody Codes settings
/// </summary>
/// <param name="Cookie">Request cookie</param>
/// <param name="LastRequestTimestamp">Last request timestamp</param>
/// <param name="Seed">Challenge seed</param>
[PublicAPI]
[method: JsonConstructor]
public sealed record EverybodyCodesSettings(string Cookie, long LastRequestTimestamp, uint? Seed)
    : ResolverSettings(Cookie, LastRequestTimestamp),
      IResolverSettings<EverybodyCodesSettings>
{
    /// <inheritdoc />
    public static EverybodyCodesSettings Default { get; } = new(string.Empty, 0L, null);

    /// <inheritdoc />
    public static JsonTypeInfo<EverybodyCodesSettings> SettingsTypeInfo => EverybodyCodesSettingsJsonContext.Default.EverybodyCodesSettings;
}
