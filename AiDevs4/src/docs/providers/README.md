# AI Model Providers

AiDevs4 supports multiple AI providers through Microsoft.Extensions.AI abstractions.

## Available Providers

| Provider | Docs | Use Case |
|----------|------|----------|
| GitHub Models | [github-models.md](./github-models.md) | Free tier, prototyping |
| OpenAI | Coming soon | Production, latest models |
| Ollama | Coming soon | Local inference, privacy |

## Quick Start

1. Configure your provider in `appsettings.json` or user secrets
2. Use `ModelConfiguration` enum to select a model
3. Access via `GetRequiredKeyedService<IChatClient>(model.CreateServiceId())`

### Configuration

API keys are configured under the `Ai` section in `appsettings.json`:

```json
{
  "Ai": {
    "OpenAi": {
      "ApiKey": "your-openai-key"
    },
    "GithubModels": {
      "ApiEndpoint": "https://models.inference.ai.azure.com",
      "ApiKey": "your-github-token"
    },
    "Ollama": {
      "Endpoint": "http://localhost:11434"
    }
  }
}
```

For local development, use User Secrets to avoid committing API keys:

```bash
# When using Aspire orchestration (recommended)
dotnet user-secrets set "github-models-apikey" "your-github-token" --project AiDevs4Aspire

# When running AiDevs4 directly
dotnet user-secrets set "Ai:GithubModels:ApiKey" "your-github-token" --project AiDevs4
```

### Available Models

The `ModelConfiguration` enum defines available models per provider:

#### OpenAI API (3 models)

| Enum Value | Model ID | Notes |
|------------|----------|-------|
| `Gpt5` | gpt-5 | Current flagship |
| `Gpt5_Mini` | gpt-5-mini | Cost-efficient |
| `Gpt4_1` | gpt-4.1 | Stable non-reasoning |

#### GitHub Models (5 models)

| Enum Value | Model ID | Notes |
|------------|----------|-------|
| `Gpt4_1_Github` | gpt-4.1 | Latest GPT-4 |
| `Gpt4_1_Mini_Github` | gpt-4.1-mini | Cost-efficient |
| `O3_Mini_Github` | o3-mini | Reasoning |
| `O4_Mini_Github` | o4-mini | Latest reasoning |
| `Phi4_Github` | phi-4 | Microsoft lightweight |

#### Ollama (3 models)

| Enum Value | Model ID | Notes |
|------------|----------|-------|
| `OllamaLlama31` | llama3.1 | Most popular (110M pulls) |
| `OllamaDeepSeekR1` | deepseek-r1 | Reasoning (78M pulls) |
| `OllamaPhi4` | phi4 | Microsoft lightweight |

### Usage in Lessons

```csharp
public class MyLesson : Lesson
{
    protected override string LessonName => "L01E01 — My Task";
    protected override Delegate GetAnswerDelegate => async (
        [FromServices] IServiceProvider serviceProvider,
        [FromQuery] string question,
        [FromQuery] ModelConfiguration model = ModelConfiguration.Gpt4_1_Mini_Github,
        CancellationToken cancellationToken = default) =>
    {
        var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());

        var response = await chatClient.CompleteAsync(
            question,
            cancellationToken: cancellationToken);

        return response.Message.Text;
    };
}
```

See individual provider docs for detailed setup instructions.
