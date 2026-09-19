using System.Text.Json.Serialization;

namespace EverybodyCodes.Resolver.Models;

/// <summary>
/// Global source generated json serialization context
/// </summary>
[JsonSerializable(typeof(User))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal sealed partial class ModelsContext : JsonSerializerContext;
