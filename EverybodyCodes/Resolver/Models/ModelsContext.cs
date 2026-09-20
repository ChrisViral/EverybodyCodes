using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Resolver.Models;

/// <summary>
/// Global source generated json serialization context
/// </summary>
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(ImmutableDictionary<int, string>))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal sealed partial class ModelsContext : JsonSerializerContext;
