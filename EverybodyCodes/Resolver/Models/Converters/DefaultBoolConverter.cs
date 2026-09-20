using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Resolver.Models.Converters;

/// <summary>
/// Default boolean value converter
/// </summary>
internal sealed class DefaultBoolConverter : JsonConverter<bool>
{
    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return reader.TokenType switch
        {
            JsonTokenType.True  => true,
            JsonTokenType.False => false,
            _                   => throw new JsonException("Excepected boolean value")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
