using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiDevs2_szkolenie.Tasks
{
    public record TaskModel(
        int Code,
        string Msg,
        string Question,
        object Input,
        string Hint,
        string? Data,
        string? Hint1,
        string? Hint2,
        List<string>? Blog)
    {
        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtraData { get; set; } = new Dictionary<string, JsonElement>();
    }
}
