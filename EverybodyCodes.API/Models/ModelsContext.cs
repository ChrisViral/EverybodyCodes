using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace EverybodyCodes.API.Models;

/// <summary>
/// Global source generated json serialization context
/// </summary>
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Quest))]
[JsonSerializable(typeof(Inputs))]
[JsonSerializable(typeof(AnswerRequest))]
[JsonSerializable(typeof(AnswerResponse))]
[JsonSerializable(typeof(Error))]
[PublicAPI, JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public sealed partial class ModelsContext : JsonSerializerContext;
