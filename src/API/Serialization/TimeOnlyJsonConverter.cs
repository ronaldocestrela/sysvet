using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Serialization;

/// <summary>Serializes <see cref="TimeOnly"/> for minimal API JSON.</summary>
public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    /// <inheritdoc />
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TimeOnly.Parse(reader.GetString() ?? string.Empty);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("HH\\:mm\\:ss"));
}
