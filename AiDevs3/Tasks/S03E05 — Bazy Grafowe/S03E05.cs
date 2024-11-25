using System.Text.Json;
using Neo4j.Driver;

namespace AiDevs3.Tasks.S03E05___Bazy_Grafowe;

public class S03E05 : Lesson
{
    private readonly IDriver _neo4JDriver;

    public S03E05(
        IConfiguration configuration,
        HttpClient httpClient,
        IDriver neo4JDriver) : base(configuration, httpClient)
    {
        _neo4JDriver = neo4JDriver;
    }

    protected override string LessonName => "S03E05 — Bazy Grafowe";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        var connections = await GetConnections(HttpClient, ApiKey, CentralaBaseUrl);
        var users = await GetUsers(HttpClient, ApiKey, CentralaBaseUrl);

        await BuildNeo4JGraph(_neo4JDriver, users, connections);
        var path = await FindShortestPath(_neo4JDriver, "Rafał", "Barbara");
        var submissionResult = await SubmitResults("connections", path);

        return TypedResults.Json(submissionResult);
    };

    private static async Task<IReadOnlyList<Connection>> GetConnections(
        HttpClient httpClient,
        string apiKey,
        string centralaBaseUrl)
    {
        var request = new
        {
            task = "database",
            apikey = apiKey,
            query = "SELECT * FROM connections"
        };

        var apiUrl = $"{centralaBaseUrl}/apidb";
        var response = await httpClient.PostAsJsonAsync(apiUrl, request);
        response.EnsureSuccessStatusCode();

        var document = await response.Content.ReadFromJsonAsync<JsonDocument>() ?? throw new InvalidOperationException("Failed to get database response");
        var apiResponse = document.RootElement.Deserialize<ApiResponse<Connection>>();
        return apiResponse?.Reply ?? throw new InvalidOperationException("Failed to deserialize connections");
    }

    private static async Task<IReadOnlyList<User>> GetUsers(
        HttpClient httpClient,
        string apiKey,
        string centralaBaseUrl)
    {
        var request = new
        {
            task = "database",
            apikey = apiKey,
            query = "SELECT * FROM users"
        };

        var apiUrl = $"{centralaBaseUrl}/apidb";
        var response = await httpClient.PostAsJsonAsync(apiUrl, request);
        response.EnsureSuccessStatusCode();

        var document = await response.Content.ReadFromJsonAsync<JsonDocument>() ?? throw new InvalidOperationException("Failed to get database response");
        var apiResponse = document.RootElement.Deserialize<ApiResponse<User>>();
        return apiResponse?.Reply ?? throw new InvalidOperationException("Failed to deserialize users");
    }

    private static async Task BuildNeo4JGraph(
        IDriver driver,
        IReadOnlyList<User> users,
        IReadOnlyList<Connection> connections)
    {
        await using var session = driver.AsyncSession();

        // Clear existing data
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync("MATCH (n) DETACH DELETE n");
        });

        // Create user nodes
        await session.ExecuteWriteAsync(async tx =>
        {
            foreach (var user in users)
            {
                await tx.RunAsync(
                    "CREATE (u:User {id: $id, username: $username})",
                    new { id = user.Id, username = user.Username }
                );
            }
        });

        // Create relationships
        await session.ExecuteWriteAsync(async tx =>
        {
            foreach (var connection in connections)
            {
                await tx.RunAsync("""
                                  MATCH (u1:User {id: $user1Id})
                                  MATCH (u2:User {id: $user2Id})
                                  CREATE (u1)-[:KNOWS]->(u2)
                                  """,
                    new { user1Id = connection.User1Id, user2Id = connection.User2Id }
                );
            }
        });
    }

    private static async Task<string> FindShortestPath(
        IDriver driver,
        string fromUser,
        string toUser)
    {
        await using var session = driver.AsyncSession();

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var query = """
                        MATCH path = shortestPath(
                            (start:User {username: $fromUser})-[:KNOWS*]-(end:User {username: $toUser})
                        )
                        RETURN [node IN nodes(path) | node.username] as usernames
                        """;

            var cursor = await tx.RunAsync(query, new { fromUser, toUser });
            var record = await cursor.SingleAsync();
            return record["usernames"].As<List<string>>();
        });

        return string.Join(", ", result);
    }
}
