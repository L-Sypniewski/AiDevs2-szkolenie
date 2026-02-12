# GitHub Models

GitHub Models provides free access to AI models through Azure's inference endpoint, ideal for prototyping and learning.

## What is GitHub Models?

GitHub Models is an AI inference API that lets you run AI models using your GitHub credentials. You can choose from models by OpenAI, Meta, DeepSeek, and others—no separate authentication process required.

**Key benefits:**
- Free tier for prototyping and development
- Access via GitHub Personal Access Token (PAT)
- Compatible with OpenAI SDK
- Works in GitHub Actions

> **Source:** [GitHub Models Quickstart](https://docs.github.com/en/github-models/quickstart)

## Token Requirements

As of **May 2025**, accessing GitHub Models requires the `models:read` permission for fine-grained Personal Access Tokens (PATs) and GitHub Apps.

> **Source:** [GitHub Changelog - May 15, 2025](https://github.blog/changelog/2025-05-15-modelsread-now-required-for-github-models-access/)

### Creating a PAT

1. Go to [GitHub Settings → Tokens](https://github.com/settings/tokens)
2. Click "Generate new token (fine-grained)"
3. Set a name (e.g., "AiDevs4 GitHub Models")
4. Under **Account permissions**, enable **Models** → **Read**
5. Generate and copy the token

> **Source:** [GitHub Models Quickstart - Step 2](https://docs.github.com/en/github-models/quickstart#step-2-make-an-api-call)

## Configuration in AiDevs4

### Running via Aspire (Recommended)

When using Aspire orchestration, configure the secret in the **AiDevs4Aspire** project:

```bash
dotnet user-secrets set "github-models-apikey" "github_pat_xxxxx" --project AiDevs4Aspire
```

Aspire passes this to AiDevs4 as the `Ai__GithubModels__ApiKey` environment variable.

### Running AiDevs4 Directly

When running AiDevs4 without Aspire:

```bash
dotnet user-secrets set "Ai:GithubModels:ApiKey" "github_pat_xxxxx" --project AiDevs4
```

Or via `appsettings.json`:

```json
{
  "Ai": {
    "GithubModels": {
      "ApiEndpoint": "https://models.inference.ai.azure.com",
      "ApiKey": "github_pat_xxxxx"
    }
  }
}
```

> **Note:** Never commit API keys to version control. Use User Secrets for local development.

## Available Models

| Enum Value | Model ID | Description |
|------------|----------|-------------|
| `Gpt4o_Github` | `gpt-4o` | GPT-4 Omni (latest) |
| `Gpt4o_Mini_Github` | `gpt-4o-mini` | GPT-4 Omni Mini (faster, cheaper) |

Browse the full catalog at [github.com/marketplace/models](https://github.com/marketplace/models)

## Usage Examples

### Basic Chat Completion

```csharp
using Microsoft.Extensions.AI;

public class MyLesson : Lesson
{
    protected override string LessonName => "L01E01 — GitHub Models Demo";
    protected override Delegate GetAnswerDelegate => async (
        [FromServices] IServiceProvider serviceProvider,
        [FromQuery] string question,
        CancellationToken cancellationToken = default) =>
    {
        // Use GitHub Models via the enum
        var model = ModelConfiguration.Gpt4o_Mini_Github;
        var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());

        var response = await chatClient.CompleteAsync(question, cancellationToken: cancellationToken);

        return response.Message.Text;
    };
}
```

### With System Prompt

```csharp
var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(
    ModelConfiguration.Gpt4o_Github.CreateServiceId());

var messages = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful coding assistant."),
    new(ChatRole.User, question)
};

var response = await chatClient.CompleteAsync(messages, cancellationToken: cancellationToken);
```

### With Function Calling (Tools)

```csharp
using Microsoft.Extensions.AI;

// Define a tool
AIFunction getCurrentWeather = AIFunctionFactory.Create(
    (string location) => $"Weather in {location}: Sunny, 22°C",
    "get_current_weather",
    "Get the current weather for a location");

// Create chat client with function invocation middleware
var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(
    ModelConfiguration.Gpt4o_Github.CreateServiceId());

var response = await chatClient.CompleteAsync(
    "What's the weather in Paris?",
    new ChatOptions { Tools = [getCurrentWeather] },
    cancellationToken: cancellationToken);
```

### Using with ChatClientAgent

```csharp
using Microsoft.Agents.AI;

var chatClient = serviceProvider.GetRequiredKeyedService<IChatClient>(
    ModelConfiguration.Gpt4o_Mini_Github.CreateServiceId());

var agent = new ChatClientAgent(
    chatClient,
    name: "CodeReviewer",
    description: "Reviews code for best practices",
    instructions: "You are a code reviewer. Analyze code and suggest improvements.",
    tools: MyTools.CreateTools());

var session = await agent.CreateSessionAsync(cancellationToken);
var response = await agent.RunAsync(codeInput, session, cancellationToken: cancellationToken);
```

## How It Works

AiDevs4 registers GitHub Models using the OpenAI SDK with a custom endpoint:

```csharp
// From AddAiClientsExtensions.cs
var openAiGithubClient = new OpenAIClient(
    new ApiKeyCredential(aiOptions.GithubModels.ApiKey),
    new OpenAIClientOptions { Endpoint = aiOptions.GithubModels.ApiEndpoint });

// Endpoint: https://models.inference.ai.azure.com
```

This approach provides:
- Full OpenAI SDK compatibility
- Standard `IChatClient` interface via Microsoft.Extensions.AI
- Middleware pipeline (logging, function invocation, telemetry)

## Rate Limits

GitHub Models has usage limits on the free tier:
- **Requests per minute:** Varies by model
- **Tokens per request:** Varies by model

Check the [GitHub Models Marketplace](https://github.com/marketplace/models) for current limits on each model.

## Using in GitHub Actions

To use GitHub Models in CI/CD workflows, add the `models: read` permission:

```yaml
name: AI Tests

on: [push]

permissions:
  models: read

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - name: Call AI model
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          curl "https://models.github.ai/inference/chat/completions" \
             -H "Content-Type: application/json" \
             -H "Authorization: Bearer $GITHUB_TOKEN" \
             -d '{"model":"openai/gpt-4o","messages":[{"role":"user","content":"Hello"}]}'
```

> **Source:** [GitHub Models Quickstart - Step 3](https://docs.github.com/en/github-models/quickstart#step-3-run-models-in-github-actions)

## Troubleshooting

### 401 Unauthorized

- Verify your PAT has **Models → Read** permission
- Check the token hasn't expired
- If using Aspire: ensure secret is set with `--project AiDevs4Aspire`
- If running directly: ensure secret is set with `--project AiDevs4`

### 429 Too Many Requests

- You've hit rate limits; wait and retry
- Consider using a smaller model (gpt-4o-mini has higher limits)

### Model Not Found

- Verify the model name matches the GitHub Models catalog
- Some models may have different names than OpenAI's API

## Sources

- [GitHub Models Quickstart](https://docs.github.com/en/github-models/quickstart)
- [About GitHub Models](https://docs.github.com/en/github-models/about-github-models)
- [GitHub Changelog: models:read permission](https://github.blog/changelog/2025-05-15-modelsread-now-required-for-github-models-access/)
- [GitHub Models Marketplace](https://github.com/marketplace/models)
- [Integrating AI Models into Development Workflow](https://docs.github.com/en/github-models/use-github-models/integrating-ai-models-into-your-development-workflow)
