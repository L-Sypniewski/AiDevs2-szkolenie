using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToImage;
using AudioContent = Microsoft.SemanticKernel.AudioContent;
using ImageContent = Microsoft.SemanticKernel.ImageContent;
using TextContent = Microsoft.SemanticKernel.TextContent;

namespace AiDevs3.AiClients.SemanticKernel;

public class SemanticKernelClient
{
    private readonly Kernel _kernel;

    public SemanticKernelClient(Kernel kernel) => _kernel = kernel;

    private static (int? MaxTokens, Dictionary<string, object> ExtensionData) GetMaxTokens(int maxTokens, ModelConfiguration model)
    {
        if (model.GetProvider() == AiProvider.GithubModels)
        {
            return (MaxTokens: null, new Dictionary<string, object>() { ["max_completion_tokens"] = maxTokens });
        }

        return (MaxTokens: maxTokens, ExtensionData: []);
    }

    public async Task<string> ExecutePrompt(
        ModelConfiguration model,
        string? systemPrompt,
        string userPrompt,
        int maxTokens,
        double temperature = 0.2,
        object? responseFormat = null,
        IDictionary<string, object>? additionalArguments = null,
        CancellationToken cancellationToken = default)
    {
        var maxTokensData = GetMaxTokens(maxTokens, model);
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ModelId = model.GetModelId(),
            ServiceId = model.CreateServiceId(),
            MaxTokens = maxTokensData.MaxTokens,
            ExtensionData = maxTokensData.ExtensionData,
            Temperature = temperature,
            ResponseFormat = responseFormat
        };
        var kernelArguments = new KernelArguments(promptExecutionSettings)
        {
            ["system"] = systemPrompt,
            ["question"] = userPrompt,
        };
        foreach (var (key, value) in additionalArguments ?? new Dictionary<string, object>())
        {
            kernelArguments[key] = value;
        }
        const string SystemMessage =
            "<message role=\"system\">{{$system}}</message>";
        const string UserMessage =
            "<message role=\"user\">{{$question}}\n</message>";
        var prompt = $"{(systemPrompt is not null ? SystemMessage : string.Empty)}\n{UserMessage}";

        // TODO: Measure prompt cache hit rate: https://platform.openai.com/docs/guides/prompt-caching
        //TODO: Optimizing LLMs for accuracy: https://platform.openai.com/docs/guides/optimizing-llm-accuracy
        var invokePromptAsync = (await _kernel.InvokePromptAsync<string>(prompt, kernelArguments, cancellationToken: cancellationToken))!;
        return invokePromptAsync;
    }

    public async Task<string> ExecuteVisionPrompt(
        ModelConfiguration model,
        string? systemPrompt,
        string userPrompt,
        IReadOnlyCollection<ReadOnlyMemory<byte>> imageData,
        int maxTokens,
        object? responseFormat = null,
        double temperature = 0.2,
        CancellationToken cancellationToken = default)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(serviceKey: model.CreateServiceId());

        var chatHistory = new ChatHistory();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            chatHistory.AddSystemMessage(systemPrompt);
        }

        var messageContent = new ChatMessageContentItemCollection
        {
            new TextContent(userPrompt)
        };

        foreach (var imageBytes in imageData)
        {
            messageContent.Add(new ImageContent(imageBytes, "image/png"));
        }

        chatHistory.AddUserMessage(messageContent);

        var maxTokensData = GetMaxTokens(maxTokens, model);
        var result = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            new OpenAIPromptExecutionSettings
            {
                ModelId = model.GetModelId(),
                ServiceId = model.CreateServiceId(),
                MaxTokens = maxTokensData.MaxTokens,
                ExtensionData = maxTokensData.ExtensionData,
                Temperature = temperature,
                ResponseFormat = responseFormat
            },
            cancellationToken: cancellationToken);

        if (result.Content is null)
        {
            throw new InvalidOperationException("Chat completion failed");
        }

        return result.Content;
    }

    public async Task<string> TranscribeAudioAsync(
        ModelConfiguration model,
        string filename,
        Stream audioStream,
        string language = "pl",
        string? prompt = null,
        float temperature = 0.0f,
        CancellationToken cancellationToken = default)
    {
        var audioToTextService = _kernel.GetRequiredService<IAudioToTextService>(serviceKey: model.CreateServiceId());

        var executionSettings = new OpenAIAudioToTextExecutionSettings(filename)
        {
            Language = language,
            ServiceId = model.GetModelId(),
            Prompt = prompt,
            ResponseFormat = "json",
            Temperature = temperature
        };

        var audioContent = new AudioContent(
            await BinaryData.FromStreamAsync(audioStream, cancellationToken),
            mimeType: null);

        var result = await audioToTextService.GetTextContentAsync(audioContent, executionSettings, cancellationToken: cancellationToken);
        if (result.Text is null)
        {
            throw new InvalidOperationException($"Audio transcription failed for file {filename}");
        }

        return result.Text;
    }

    public enum DallE3ImageSize
    {
        Square1024 = 0, // 1024x1024
        Landscape1792 = 1, // 1792x1024
        Portrait1792 = 2 // 1024x1792
    }

    public enum DallE3Quality
    {
        Standard,
        HD
    }

    public async Task<string> ExecuteDalle3ImagePrompt(
        string prompt,
        DallE3ImageSize size,
        DallE3Quality quality,
        CancellationToken cancellationToken = default)
    {
        const string Dalle3Model = "dall-e-3";

        var textToImageService =
            _kernel.GetRequiredService<ITextToImageService>(serviceKey: ModelConfiguration.Dalle3.CreateServiceId());

        var executionSettings = new OpenAITextToImageExecutionSettings
        {
            ModelId = Dalle3Model,
            ServiceId = Dalle3Model,
            Size = GetImageDimensions(size),
            Quality = quality == DallE3Quality.Standard ? "standard" : "hd"
        };

        return (await textToImageService.GetImageContentsAsync(new TextContent(prompt), executionSettings, cancellationToken: cancellationToken))[0].Uri!
            .ToString();

        static (int Width, int Height) GetImageDimensions(DallE3ImageSize size) =>
            size switch
            {
                DallE3ImageSize.Square1024 => (1024, 1024),
                DallE3ImageSize.Landscape1792 => (1792, 1024),
                DallE3ImageSize.Portrait1792 => (1024, 1792),
                _ => throw new ArgumentOutOfRangeException(nameof(size))
            };
    }
}
