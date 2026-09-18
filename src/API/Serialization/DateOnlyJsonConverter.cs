using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Serialization;

/// <summary>Serializes <see cref="DateOnly"/> for minimal API JSON responses.</summary>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    /// <inheritdoc />
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateOnly.Parse(reader.GetString() ?? string.Empty);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
}

/// <summary>Serializes nullable <see cref="DateOnly"/> values.</summary>
public sealed class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    /// <inheritdoc />
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();
        return string.IsNullOrWhiteSpace(text) ? null : DateOnly.Parse(text);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
    }
}
