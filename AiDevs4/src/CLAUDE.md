# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build the project
dotnet build

# Run the application (API on http://localhost:5050 with Swagger UI)
dotnet run

# Run with watch for development
dotnet watch
```

## Architecture Overview

This is an educational platform for implementing AI agents using Microsoft.Extensions.AI. The architecture follows a modular pattern with auto-discovery.

### Module System

- **IModule interface** (`DependencyInjection/IModule.cs`): All modules implement `RegisterModule()` and `MapEndpoints()`
- **Auto-discovery**: `ModuleExtensions.RegisterModules()` scans the assembly for all `IModule` implementations and registers them automatically
- **Endpoint mapping**: `app.MapEndpoints()` calls `MapEndpoints()` on all discovered modules

### Creating New Lessons

1. Create a new folder under `Tasks/` (e.g., `L01E02_MyTask/`)
2. Create a class inheriting from `Lesson` abstract class
3. Override `LessonName` and `GetAnswerDelegate` (automatically creates `/answer` endpoint)
4. Optionally override `MapAdditionalEndpoints()` for custom endpoints

```csharp
public class L01E02_MyTask : Lesson
{
    protected override string LessonName => "L01E02 — My Task";
    protected override Delegate GetAnswerDelegate => async (
        [FromServices] IServiceProvider serviceProvider,
        [FromQuery] string question,
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt4_1_Mini_Github,
        CancellationToken cancellationToken = default) =>
    {
        // Implementation
    };
}
```

### AI Client System

- **Multi-provider support**: OpenAI, GitHub Models (Azure), and Ollama
- **Keyed services**: AI clients registered as keyed `IChatClient` using `{modelId}-{provider}` pattern
- **Access pattern**: `serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId())`
- **Available models**: See `ModelConfiguration` enum in `AiClients/ModelConfigurations.cs`

### Tool Pattern

Tools are static methods converted to `AITool` via `AIFunctionFactory.Create()`:

```csharp
public static IList<AITool> CreateTools() =>
[
    AIFunctionFactory.Create(MethodName, "ToolName", "Description")
];
```

Existing tools in `Tools/`:
- **FileReader**: Read files, list directories
- **Unzipper**: Extract/list ZIP archives
- **UrlFetcher**: Fetch data from URLs (requires HttpClient injection)

### Agent Pattern

Uses `ChatClientAgent` from Microsoft.Agents.AI:

```csharp
var agent = new ChatClientAgent(
    chatClient,
    name: "AgentName",
    description: "Agent description",
    instructions: "System prompt here",
    tools: toolList);

var session = await agent.CreateSessionAsync(cancellationToken);
var response = await agent.RunAsync(input, session, cancellationToken: cancellationToken);
```

## Configuration

API keys are configured in `appsettings.json` under the `Ai` section:
- `OpenAi.ApiKey` - OpenAI API key
- `GithubModels.ApiKey` - GitHub Models (Azure) API key
- `Ollama.Endpoint` - Local Ollama endpoint (default: http://localhost:11434)
