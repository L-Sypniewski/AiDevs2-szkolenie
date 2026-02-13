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
        [FromServices] HttpClient httpClient,
        [FromServices] ILoggerFactory loggerFactory,
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt5_Mini_Github,
        CancellationToken cancellationToken = default) =>
    {
        var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());
        var urlFetcherTool = new UrlFetcherTool(httpClient);

        // Aggregate all tools from Tools directory
        var tools = new List<AITool>();
        tools.AddRange(FileReaderTool.CreateTools());
        tools.AddRange(UnzipperTool.CreateTools());
        tools.AddRange(urlFetcherTool.CreateTools());

        var agent = new ChatClientAgent(
            chatClient,
            name: "ToolUsingAgent",
            instructions: "You are a helpful assistant with access to file system, URL fetching, and ZIP tools. You can be tasked with open ended questions, and you should use the tools at your disposal to find the answer. Always think step by step and explain your reasoning. If you encounter error you cannot recover from, explain the error and end the session.",
            loggerFactory: loggerFactory,
            tools: tools);

        var task = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Tasks", "assets", "Task.md"), cancellationToken);
        var session = await agent.CreateSessionAsync(cancellationToken);
        var response = await agent.RunAsync(task, session, cancellationToken: cancellationToken);

        return Results.Ok(new { model, task, answer = response.Text });
    };
}
