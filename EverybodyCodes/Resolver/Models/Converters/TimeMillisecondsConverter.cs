using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Resolver.Models.Converters;

/// <summary>
/// Milliseconds to <see cref="TimeSpan"/> converter
/// </summary>
public sealed class TimeMillisecondsConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType is JsonTokenType.Number
                   ? TimeSpan.FromMilliseconds(reader.GetInt64())
                   : throw new JsonException("Expected numerical value");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Ticks / TimeSpan.TicksPerMillisecond);
    }
}
