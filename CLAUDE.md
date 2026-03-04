# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Structure

This is an educational training ground for AI development, organized into multiple .NET projects representing different course iterations:

| Project | Framework | AI Library | Purpose |
|---------|-----------|------------|---------|
| **AiDevs2** | .NET 8/9 | Semantic Kernel | Course 2 exercises (C01-C05) |
| **AiDevs3** | .NET 9/10 | Semantic Kernel + Microsoft.Extensions.AI | Course 3 exercises (S01-S04), Aspire-enabled |
| **AiDevs4** | .NET 10 / C# 14 | Microsoft.Extensions.AI + Microsoft.Agents.AI | Course 4 exercises, modern architecture |
| **AiDevs4Aspire** | .NET Aspire | - | AiDevs4 with Aspire orchestration (GitHub Models secrets) |
| **AiDevs.AppHost** | .NET Aspire | - | Development orchestration for AiDevs3 (Qdrant, Ollama, Neo4j) |
| **AiDevs.ServiceDefaults** | .NET Aspire | - | Shared telemetry, service discovery, resilience |

Solution file: `AiDevs.slnx` (Visual Studio solution format)

## Build & Run Commands

```bash
# Build all projects
dotnet build

# Run individual projects
dotnet run --project AiDevs2/AiDevs2-szkolenie.csproj        # http://localhost:5000
dotnet run --project AiDevs3/AiDevs3.csproj                  # http://localhost:5000
dotnet run --project AiDevs4/src/AiDevs4.csproj              # http://localhost:5050

# Run with Aspire orchestration
dotnet run --project AiDevs4/aspire/AiDevs4Aspire.csproj     # AiDevs4 + GitHub Models secrets
dotnet run --project AiDevs.AppHost/AiDevs.AppHost.csproj    # AiDevs3 + Qdrant, Ollama, Neo4j

# Development with hot reload
dotnet watch --project AiDevs4/src/AiDevs4.csproj
```

## Using dotnet CLI

Use `dotnet` CLI for building, running, adding/updating/removing NuGet packages, formatting code (`dotnet format`), and adding project references.

Docs: https://learn.microsoft.com/en-us/dotnet/core/tools/

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
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt4_1_Mini_Github,
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
- **Available models** (AiDevs4 `ModelConfiguration` enum):
  - OpenAI: `Gpt5`, `Gpt5_Mini`, `Gpt4_1`
  - GitHub Models: `Gpt4_1_Github`, `Gpt4_1_Mini_Github`, `O3_Mini_Github`, `O4_Mini_Github`, `Phi4_Github`
  - Ollama: `OllamaLlama31`, `OllamaDeepSeekR1`, `OllamaPhi4`

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
# When running AiDevs4 directly
dotnet user-secrets set "Ai:GithubModels:ApiKey" "your-token" --project AiDevs4/src
dotnet user-secrets set "Ai:OpenAi:ApiKey" "your-key" --project AiDevs4/src

# When using AiDevs4Aspire (Aspire orchestration)
dotnet user-secrets set "github-models-apikey" "your-token" --project AiDevs4/aspire
```

## .NET Aspire Infrastructure

**AiDevs4Aspire** (recommended for AiDevs4):
- GitHub Models API key injected as secret parameter
- Optional: Qdrant, Ollama (commented out in AppHost.cs)

**AiDevs.AppHost** (for AiDevs3):
- **Qdrant** (vector DB): Dashboard at port 33267, data persisted in `rag3` volume
- **Ollama**: `ollama-embeddings` (mxbai-embed-large), `ollama-phi` (phi3:mini)
- **Neo4j**: Graph database with persistent lifetime

## Coding Conventions

From `.editorconfig`:
- **Constants**: Use PascalCase (e.g., `MaxRetries`, not `MAX_RETRIES`)
- **Private static readonly fields**: Prefix with `s_` (e.g., `s_modelMappings`)
- **Line length**: 160 characters max
- **Braces**: Required for all control statements (if, for, foreach, while, etc.)
- **Indentation**: 4 spaces, CRLF line endings
- **Modifier order**: `internal, private, protected, public, new, abstract, virtual, override, sealed, static, async...`

## Key Dependencies

- **Microsoft.Extensions.AI** - Abstractions for AI clients (v10.x in AiDevs4)
- **Microsoft.SemanticKernel** - AI orchestration framework (v1.30 in AiDevs2/3)
- **Microsoft.Agents.AI** - Agent framework (preview in AiDevs4)
- **Aspire.Hosting** - Development orchestration (v9.0 in AppHosts)
