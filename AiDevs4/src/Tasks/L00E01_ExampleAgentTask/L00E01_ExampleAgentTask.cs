using AiDevs4.AiClients;
using AiDevs4.Tools.FileReader;
using AiDevs4.Tools.Unzipper;
using AiDevs4.Tools.UrlFetcher;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace AiDevs4.Tasks.L00E01_ExampleAgentTask;

public class L00E01_ExampleAgentTask : Lesson
{
    protected override string LessonName => "L00E01 — Example Agent Task";

    protected override Delegate GetAnswerDelegate => async (
        [FromServices] IServiceProvider serviceProvider,
        [FromQuery] string question,
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt4_1_Mini_Github,
        CancellationToken cancellationToken = default) =>
    {
        var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());

        var agent = new ChatClientAgent(
            chatClient,
            name: "ResearchAssistant",
            description: "A helpful research assistant",
            instructions: """
                          You are a helpful research assistant. When answering questions:
                          1. Think step by step
                          2. Provide clear and concise answers
                          3. If you use tools, explain what you found
                          4. Always cite your reasoning
                          """,
            tools: null);

        var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);
        var response = await agent.RunAsync(question, session, cancellationToken: cancellationToken);

        return Results.Ok(new
        {
            Model = model.GetModelId(),
            Provider = model.GetProvider().ToString(),
            Question = question,
            Answer = response.Text
        });
    };

    protected override void MapAdditionalEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("agent-with-tools", AgentWithToolsDelegate)
            .WithName("Agent with tools: Example Agent Task");

        endpoints.MapGet("file-agent", FileAgentDelegate)
            .WithName("File agent: Example Agent Task");
    }

    private static Delegate AgentWithToolsDelegate => async (
        [FromServices] IServiceProvider serviceProvider,
        [FromQuery] string question,
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt4_1_Mini_Github,
        CancellationToken cancellationToken = default) =>
    {
        var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());

        var agent = new ChatClientAgent(
            chatClient,
            name: "ToolUsingAssistant",
            description: "A helpful assistant that can use tools",
            instructions: "You are a helpful assistant that can use tools to answer questions. Use the available tools when appropriate.",
            tools:
            [
                AIFunctionFactory.Create((string city) =>
                {
                    return city.ToLowerInvariant() switch
                    {
                        "warsaw" or "warszawa" => "15°C, partly cloudy",
                        "london" => "12°C, rainy",
                        "new york" => "20°C, sunny",
                        "tokyo" => "22°C, clear",
                        _ => $"Weather data not available for {city}"
                    };
                }, "GetWeather", "Gets the current weather for a specified city"),

                AIFunctionFactory.Create((double a, double b, string operation) =>
                {
                    return operation.ToLowerInvariant() switch
                    {
                        "add" => $"{a} + {b} = {a + b}",
                        "subtract" => $"{a} - {b} = {a - b}",
                        "multiply" => $"{a} * {b} = {a * b}",
                        "divide" when b != 0 => $"{a} / {b} = {a / b}",
                        "divide" => "Cannot divide by zero",
                        _ => $"Unknown operation: {operation}"
                    };
                }, "Calculate", "Performs a basic math calculation (add, subtract, multiply, divide)")
            ]);

        var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);
        var response = await agent.RunAsync(question, session, cancellationToken: cancellationToken);

        return Results.Ok(new
        {
            Model = model.GetModelId(),
            Provider = model.GetProvider().ToString(),
            Question = question,
            Answer = response.Text
        });
    };

    private static Delegate FileAgentDelegate => async (
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] HttpClient httpClient,
        [FromQuery] string question,
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt4_1_Mini_Github,
        CancellationToken cancellationToken = default) =>
    {
        var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());
        var urlFetcherTool = new UrlFetcherTool(httpClient);

        var tools = new List<AITool>();
        tools.AddRange(UnzipperTool.CreateTools());
        tools.AddRange(FileReaderTool.CreateTools());
        tools.AddRange(urlFetcherTool.CreateTools());

        var agent = new ChatClientAgent(
            chatClient,
            name: "FileProcessingAgent",
            description: "An agent that can fetch URLs, unzip archives, and read text files",
            instructions: """
                          You are a file processing assistant. You can:
                          1. Fetch data from URLs
                          2. Download files from URLs
                          3. Extract ZIP archives
                          4. List ZIP archive contents
                          5. Read text files
                          6. List directory contents
                          Use these tools to accomplish the user's request step by step.
                          """,
            tools: tools);

        var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);
        var response = await agent.RunAsync(question, session, cancellationToken: cancellationToken);

        return Results.Ok(new
        {
            Model = model.GetModelId(),
            Provider = model.GetProvider().ToString(),
            Question = question,
            Answer = response.Text
        });
    };
}
