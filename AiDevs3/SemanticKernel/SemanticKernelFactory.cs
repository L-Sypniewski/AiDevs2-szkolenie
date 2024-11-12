using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace AiDevs3.SemanticKernel;

public class SemanticKernelFactory
{
    private enum AiProvider
    {
        OpenAI,
        GithubModels
    }

    private readonly SemanticKernelFactoryOptions _semanticKernelFactoryOptions;

    public SemanticKernelFactory(IOptions<SemanticKernelFactoryOptions> options) => _semanticKernelFactoryOptions = options.Value;

    public Kernel BuildSemanticKernel(string? promptDirectory = null)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole());

        var openAiClient = CreateOpenAIClient(AiProvider.OpenAI);
        var githubModelsClient = CreateOpenAIClient(AiProvider.GithubModels);

        builder.Services
            .AddOpenAIChatCompletion(modelId: "gpt-4o-2024-08-06", openAiClient, serviceId: "gpt-4o-2024-08-06")
            .AddOpenAIChatCompletion(modelId: "gpt-4o-mini-2024-07-18", openAiClient, serviceId: "gpt-4o-mini-2024-07-18")
            .AddOpenAIChatCompletion(modelId: "gpt-4-turbo-2024-04-09", openAiClient, serviceId: "gpt-4-turbo-2024-04-09")
            // .AddOpenAIChatCompletion(modelId: "o1-mini-2024-09-12", openAiClient, serviceId: "o1-mini-2024-09-12")
            // .AddOpenAIChatCompletion(modelId: "o1-preview-2024-09-12", openAiClient, serviceId: "o1-preview-2024-09-12")
            .AddOpenAIChatCompletion(modelId: "gpt-4o", githubModelsClient, serviceId: "gpt-4o")
            .AddOpenAIChatCompletion(modelId: "gpt-4o-mini", githubModelsClient, serviceId: "gpt-4o-mini")
            .AddOpenAIChatCompletion(modelId: "o1-mini", githubModelsClient, serviceId: "o1-mini")
            .AddOpenAIChatCompletion(modelId: "o1-preview", githubModelsClient, serviceId: "o1-preview")
            .AddOpenAIChatCompletion(modelId: "Phi-3.5-mini-instruct", githubModelsClient, serviceId: "Phi-3.5-mini-instruct")
            .AddOpenAIChatCompletion(modelId: "Phi-3.5-MoE-instruct", githubModelsClient, serviceId: "Phi-3.5-MoE-instruct")
            .AddOpenAIChatCompletion(modelId: "Phi-3.5-vision-instruct", githubModelsClient, serviceId: "Phi-3.5-vision-instruct")
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
