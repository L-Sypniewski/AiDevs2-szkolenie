using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace AiDevs3.SemanticKernel;

public enum AiProvider
{
    OpenAI,
    GithubModels
}

public class SemanticKernelFactory
{
    private readonly SemanticKernelFactoryOptions _semanticKernelFactoryOptions;

    public SemanticKernelFactory(IOptions<SemanticKernelFactoryOptions> options) => _semanticKernelFactoryOptions = options.Value;

    public Kernel BuildSemanticKernel(string? promptDirectory = null)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole());

        var openAiClient = CreateOpenAIClient(AiProvider.OpenAI);
        var githubModelsClient = CreateOpenAIClient(AiProvider.GithubModels);

        builder.Services
            .AddOpenAIChatCompletion(modelId: "gpt-4o-2024-08-06", openAiClient, serviceId: nameof(AiProvider.OpenAI))
            .AddOpenAIChatCompletion(modelId: "gpt-4o-mini-2024-07-18", openAiClient, serviceId: nameof(AiProvider.OpenAI))
            .AddOpenAIChatCompletion(modelId: "gpt-4-turbo-2024-04-09", openAiClient, serviceId: nameof(AiProvider.OpenAI))
            // .AddOpenAIChatCompletion(modelId: "o1-mini-2024-09-12", openAiClient, serviceId: nameof(AiProvider.OpenAI))
            // .AddOpenAIChatCompletion(modelId: "o1-preview-2024-09-12", openAiClient, serviceId: nameof(AiProvider.OpenAI))
            .AddOpenAIChatCompletion(modelId: "gpt-4o", githubModelsClient, serviceId: nameof(AiProvider.GithubModels))
            .AddOpenAIChatCompletion(modelId: "gpt-4o-mini", githubModelsClient, serviceId: nameof(AiProvider.GithubModels))
            .AddOpenAIChatCompletion(modelId: "o1-mini", githubModelsClient, serviceId: nameof(AiProvider.GithubModels))
            .AddOpenAIChatCompletion(modelId: "o1-preview", githubModelsClient, serviceId: nameof(AiProvider.GithubModels))
            .AddOpenAIChatCompletion(modelId: "Phi-3.5-mini-instruct", githubModelsClient, serviceId: nameof(AiProvider.GithubModels))
            .AddOpenAIChatCompletion(modelId: "Phi-3.5-MoE-instruct", githubModelsClient, serviceId: nameof(AiProvider.GithubModels))
            .AddOpenAIAudioToText(modelId: "whisper-1", apiKey: _semanticKernelFactoryOptions.OpenAi.ApiKey);

        var kernel = builder.Build();
        if (promptDirectory is not null)
        {
            kernel.ImportPluginFromPromptDirectory(promptDirectory);
        }

        return kernel;
    }

    private OpenAIClient CreateOpenAIClient(AiProvider aiProvider)
    {
        return aiProvider switch
        {
            AiProvider.OpenAI => new OpenAIClient(
                new ApiKeyCredential(_semanticKernelFactoryOptions.OpenAi.ApiKey)),
            AiProvider.GithubModels => new OpenAIClient(
                new ApiKeyCredential(_semanticKernelFactoryOptions.GithubModels.ApiKey),
                new OpenAIClientOptions { Endpoint = _semanticKernelFactoryOptions.GithubModels.ApiEndpoint }),
            _ => throw new ArgumentOutOfRangeException(nameof(aiProvider), aiProvider, "Unsupported AI provider")
        };
    }

    public virtual async Task<string> InvokePluginWithStructuredOutputAsync(
        string promptDirectory,
        string functionName,
        Dictionary<string, object> parameters)
    {
        var kernel = BuildSemanticKernel(promptDirectory);
        var kernelFunction = kernel.Plugins["Prompts"][functionName];
        var executionSettings = GetExecutionSettings(kernelFunction);

        var kernelArguments = new KernelArguments(executionSettings);
        foreach (var (key, value) in parameters)
        {
            kernelArguments.Add(key, value);
        }

        var llmResponse = await kernel.InvokeAsync(kernelFunction, kernelArguments);

        return llmResponse.GetValue<string>()!;
    }

    private static OpenAIPromptExecutionSettings GetExecutionSettings(KernelFunction kernelFunction)
    {
        var kernelFunctionExecutionSettings = kernelFunction.ExecutionSettings!.Values.Single().ExtensionData!;
        return new OpenAIPromptExecutionSettings
        {
            Temperature = ((JsonElement) kernelFunctionExecutionSettings["temperature"]).GetDouble(),
            MaxTokens = ((JsonElement) kernelFunctionExecutionSettings["max_tokens"]).GetInt32(),
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "speaker_identification",
                jsonSchema: BinaryData.FromString(((JsonElement) kernelFunctionExecutionSettings["response_format"]).GetProperty("json_schema")
                    .GetProperty("schema")
                    .ToString()),
                jsonSchemaIsStrict: true)
        };
    }
}
