using System.Text.Json.Serialization;

namespace EverybodyCodes.Resolver.Models;

/// <summary>
/// Global source generated json serialization context
/// </summary>
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Quest))]
[JsonSerializable(typeof(Inputs))]
[JsonSerializable(typeof(AnswerRequest))]
[JsonSerializable(typeof(AnswerResponse))]
[JsonSerializable(typeof(Error))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal sealed partial class ModelsContext : JsonSerializerContext;
