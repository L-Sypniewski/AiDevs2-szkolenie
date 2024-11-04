using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;

namespace AiDevs3.SemanticKernel;

public class SemanticKernelFactory
{
    private readonly SemanticKernelFactoryOptions _semanticKernelFactoryOptions;

    public SemanticKernelFactory(IOptions<SemanticKernelFactoryOptions> options) => _semanticKernelFactoryOptions = options.Value;

    public Kernel BuildSemanticKernel(string model, string? promptDirectory = null)
    {
        var builder = Kernel.CreateBuilder();
        var openAiApiKey = _semanticKernelFactoryOptions.ApiKey;

        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole());

        var localApiEndpoint = _semanticKernelFactoryOptions.ApiEndpoint;
        if (localApiEndpoint is null)
        {
            builder.Services
                .AddOpenAIChatCompletion(model, apiKey: openAiApiKey)
                .AddOpenAIAudioToText(modelId: "whisper-1", apiKey: openAiApiKey);
        }
        else
        {
            builder.Services
                .AddOpenAIChatCompletion(model, endpoint: localApiEndpoint, apiKey: openAiApiKey);
        }

        var kernel = builder.Build();
        if (promptDirectory is not null)
        {
            kernel.ImportPluginFromPromptDirectory(promptDirectory);
        }

        return kernel;
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
