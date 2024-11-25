using System.Text.Json.Serialization;

namespace AiDevs3.Tasks.S03E05___Bazy_Grafowe;

public record ApiResponse<T>(
    [property: JsonPropertyName("reply")]
    List<T> Reply);

public record User(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("access_level")]
    string AccessLevel,
    [property: JsonPropertyName("is_active")]
    string IsActive,
    [property: JsonPropertyName("lastlog")]
    string LastLog);

public record Connection(
    [property: JsonPropertyName("user1_id")]
    string User1Id,
    [property: JsonPropertyName("user2_id")]
    string User2Id);

public record GraphData(
    [property: JsonPropertyName("users")]
    IReadOnlyList<User> Users,
    [property: JsonPropertyName("connections")]
    IReadOnlyList<Connection> Connections);

