using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Resolver.Models.Converters;

/// <summary>
/// Converter treating empty strings as null values for <see cref="Uri"/>
/// </summary>
internal sealed class EmptyUriConverter : JsonConverter<Uri?>
{
    /// <inheritdoc />
    public override Uri? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null) throw new JsonException("Expected string");
        string? value = reader.GetString();
        return !string.IsNullOrEmpty(value) ? new Uri(value) : null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Uri? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString() ?? string.Empty);
    }
}
