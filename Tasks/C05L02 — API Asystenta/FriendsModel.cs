using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiDevs2_szkolenie.Tasks;

public class FriendsModel
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtraData { get; init; } = new Dictionary<string, JsonElement>();

    public Dictionary<string, string[]> Friends => ExtraData.Keys.Select(key => key)
        .ToDictionary(key => key, key => ExtraData[key].EnumerateArray().Select(k => k.GetString()!).ToArray());
}
