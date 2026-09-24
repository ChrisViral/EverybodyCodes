using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.API.Models.Converters;

/// <summary>
/// Numerical value (0/1) to <see cref="bool"/> converter
/// </summary>
internal sealed class NumericalBoolConverter : JsonConverter<bool>
{
    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.Number) throw new JsonException("Expected numerical value");

        int value = reader.GetInt32();
        return value switch
        {
            0 => false,
            1 => true,
            _ => throw new JsonException($"Invalid boolean numerical {value}")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
