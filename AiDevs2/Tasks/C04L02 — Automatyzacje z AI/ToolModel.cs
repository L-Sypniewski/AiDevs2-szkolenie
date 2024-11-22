using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace AiDevs2_szkolenie.Tasks;

public enum Tool
{
    Calendar,
    ToDo
}

public record ToolModel
{
    [JsonPropertyName("tool")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Tool Tool { get; init; }

    [JsonPropertyName("desc")]
    public required string Desc { get; init; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; init; }

    public static JsonSerializerOptions SerializerOptions => new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = { new CustomDateTimeConverter() },
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
}

public class CustomDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTime?));

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (!reader.TryGetDateTime(out var value))
        {
            value = DateTime.Parse(reader.GetString()!);
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value!.Value.ToString("yyyy-MM-dd"));
    }
}
