# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Structure

This is an educational training ground for AI development, organized into multiple .NET projects representing different course iterations:

| Project | Framework | AI Library | Purpose |
|---------|-----------|------------|---------|
| **AiDevs2** | .NET 8/9 | Semantic Kernel | Course 2 exercises (C01-C05) |
| **AiDevs3** | .NET 9/10 | Semantic Kernel + Microsoft.Extensions.AI | Course 3 exercises (S01-S04), Aspire-enabled |
| **AiDevs4** | .NET 10 | Microsoft.Extensions.AI + Microsoft.Agents.AI | Course 4 exercises, modern architecture |
| **AiDevs.AppHost** | .NET Aspire | - | Development orchestration (Qdrant, Ollama, Neo4j) |
| **AiDevs.ServiceDefaults** | .NET Aspire | - | Shared telemetry, service discovery, resilience |

## Build & Run Commands

```bash
# Build all projects
dotnet build

# Run individual projects
dotnet run --project AiDevs2/AiDevs2-szkolenie.csproj        # http://localhost:5000
dotnet run --project AiDevs3/AiDevs3.csproj                  # http://localhost:5000
dotnet run --project AiDevs4/src/AiDevs4.csproj              # http://localhost:5050

# Run with Aspire orchestration (includes Qdrant, Ollama, Neo4j)
dotnet run --project AiDevs.AppHost/AiDevs.AppHost.csproj

# Development with hot reload
dotnet watch --project AiDevs4/src/AiDevs4.csproj
```

## Architecture Patterns

### Module Auto-Discovery System

All projects share a common module pattern:

1. **IModule interface** - Defines `RegisterModule()` and `MapEndpoints()` methods
2. **Auto-discovery** - `RegisterModules(assembly)` scans for all IModule implementations
3. **Endpoint mapping** - `MapEndpoints()` is called on the WebApplication

Each lesson/task inherits from `Lesson` abstract class which:
- Provides the `/answer` endpoint automatically via `GetAnswerDelegate`
- Optionally exposes additional endpoints via `MapAdditionalEndpoints()`

### Creating New Lessons (AiDevs4 pattern)

```csharp
public class L01E02_MyTask : Lesson
{
    protected override string LessonName => "L01E02 — My Task";
    protected override Delegate GetAnswerDelegate => async (
        [FromServices] IServiceProvider serviceProvider,
        [FromQuery] string question,
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt4o_Mini_202407,
        CancellationToken cancellationToken = default) =>
    {
        var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());
        // Implementation
    };
}
```

### AI Client System

**Multi-provider support**: OpenAI, GitHub Models (Azure), Ollama

- **Keyed services**: AI clients registered as keyed `IChatClient` using `{modelId}-{provider}` pattern
- **Access pattern**: `serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId())`
- **Available models**: See `ModelConfiguration` enum in each project's `AiClients/` folder

### Agent Pattern (AiDevs4)

Uses `ChatClientAgent` from Microsoft.Agents.AI:

```csharp
var agent = new ChatClientAgent(chatClient, name: "AgentName", instructions: "...", tools: toolList);
var session = await agent.CreateSessionAsync(cancellationToken);
var response = await agent.RunAsync(input, session, cancellationToken: cancellationToken);
```

### Tool Pattern (AiDevs4)

Tools are static methods converted to `AITool` via `AIFunctionFactory.Create()`:

```csharp
public static IList<AITool> CreateTools() =>
[
    AIFunctionFactory.Create(MethodName, "ToolName", "Description")
];
```

Existing tools in `AiDevs4/src/Tools/`: FileReader, Unzipper, UrlFetcher

## Configuration

API keys configured in `appsettings.json`:

```json
{
  "Ai": {
    "OpenAi": { "ApiKey": "..." },
    "GithubModels": { "ApiKey": "...", "ApiEndpoint": "https://models.inference.ai.azure.com" },
    "Ollama": { "Endpoint": "http://localhost:11434" }
  }
}
```

Use User Secrets for local development:
```bash
dotnet user-secrets set "Ai:OpenAi:ApiKey" "your-key" --project AiDevs4/src
```

## .NET Aspire Infrastructure

When running via AppHost, the following services are available:
- **Qdrant** (vector DB): Dashboard at port 33267, data persisted in `rag3` volume
- **Ollama**: `ollama-embeddings` (mxbai-embed-large), `ollama-phi` (phi3:mini)
- **Neo4j**: Graph database with persistent lifetime

## Key Dependencies

- **Microsoft.Extensions.AI** - Abstractions for AI clients (v10.x in AiDevs4)
- **Microsoft.SemanticKernel** - AI orchestration framework (v1.30 in AiDevs2/3)
- **Microsoft.Agents.AI** - Agent framework (preview in AiDevs4)
- **Aspire.Hosting** - Development orchestration (v9.0 in AppHost)
