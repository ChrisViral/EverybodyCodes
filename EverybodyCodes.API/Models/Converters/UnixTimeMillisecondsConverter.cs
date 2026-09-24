using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.API.Models.Converters;

/// <summary>
/// Unix millisecond timestamp to <see cref="DateTimeOffset"/> converter
/// </summary>
internal sealed class UnixTimeMillisecondsConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType is JsonTokenType.Number
                   ? DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64())
                   : throw new JsonException("Expected numerical value");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeMilliseconds());
    }
}
