using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextToImage;
using static AiDevs3.SemanticKernel.SemanticKernelFactory;

namespace AiDevs3.SemanticKernel;

public class SemanticKernelClient
{
    private readonly SemanticKernelFactory _kernelFactory;

    public SemanticKernelClient(SemanticKernelFactory semanticKernelFactory) => _kernelFactory = semanticKernelFactory;

    public async Task<string> ExecutePrompt(
        string model,
        AiProvider aiProvider,
        string? systemPrompt,
        string userPrompt,
        int maxTokens,
        double temperature = 0.2,
        object? responseFormat = null)
    {
        var kernel = _kernelFactory.BuildSemanticKernel();
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ModelId = model,
            ServiceId = SemanticKernelFactory.CreateServiceId(model, aiProvider),
            MaxTokens = maxTokens,
            Temperature = temperature,
            ResponseFormat = responseFormat
        };
        var kernelArguments = new KernelArguments(promptExecutionSettings)
        {
            ["system"] = systemPrompt,
            ["question"] = userPrompt,
        };
        const string SystemMessage =
            "<message role=\"system\">{{$system}}</message>";
        const string UserMessage =
            "<message role=\"user\">{{$question}}\n</message>";
        var prompt = $"{(systemPrompt is not null ? SystemMessage : string.Empty)}\n{UserMessage}";

        // TODO: Measure prompt cache hit rate: https://platform.openai.com/docs/guides/prompt-caching
        //TODO: Optimizing LLMs for accuracy: https://platform.openai.com/docs/guides/optimizing-llm-accuracy
        return (await kernel.InvokePromptAsync<string>(prompt, kernelArguments))!;
    }

    public async Task<string> ExecuteVisionPrompt(
        string model,
        AiProvider aiProvider,
        string? systemPrompt,
        string userPrompt,
        IReadOnlyCollection<ReadOnlyMemory<byte>> imageData,
        int maxTokens,
        double temperature = 0.2)
    {
        var kernel = _kernelFactory.BuildSemanticKernel();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>(serviceKey: SemanticKernelFactory.CreateServiceId(model, aiProvider));

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

        var result = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            new OpenAIPromptExecutionSettings
            {
                ModelId = model,
                ServiceId = model,
                MaxTokens = maxTokens,
                Temperature = temperature
            });

        if (result.Content is null)
        {
            throw new InvalidOperationException("Chat completion failed");
        }

        return result.Content;
    }

    public async Task<string> TranscribeAudioAsync(
        string model,
        AiProvider aiProvider,
        string filename,
        Stream audioStream,
        string language = "pl",
        string? prompt = null,
        float temperature = 0.0f)
    {
        var kernel = _kernelFactory.BuildSemanticKernel();
        var audioToTextService = kernel.GetRequiredService<IAudioToTextService>(serviceKey: SemanticKernelFactory.CreateServiceId(model, aiProvider));

        var executionSettings = new OpenAIAudioToTextExecutionSettings(filename)
        {
            Language = language,
            ServiceId = model,
            Prompt = prompt,
            ResponseFormat = "json",
            Temperature = temperature
        };

        var audioContent = new AudioContent(
            await BinaryData.FromStreamAsync(audioStream),
            mimeType: null);

        var result = await audioToTextService.GetTextContentAsync(audioContent, executionSettings);
        if (result.Text is null)
        {
            throw new InvalidOperationException($"Audio transcription failed for file {filename}");
        }

        return result.Text;
    }

    public enum DallE3ImageSize
    {
        Square1024 = 0,      // 1024x1024
        Landscape1792 = 1,   // 1792x1024
        Portrait1792 = 2     // 1024x1792
    }

    public enum DallE3Quality
    {
        Standard,
        HD
    }

    public async Task<string> ExecuteDalle3ImagePrompt(
        string prompt,
        DallE3ImageSize size,
        DallE3Quality quality)
    {
        const string Dalle3Model = "dall-e-3";

        var kernel = _kernelFactory.BuildSemanticKernel();
        var textToImageService = kernel.GetRequiredService<ITextToImageService>(serviceKey: SemanticKernelFactory.CreateServiceId(Dalle3Model, AiProvider.OpenAI));

        var executionSettings = new OpenAITextToImageExecutionSettings
        {
            ModelId = Dalle3Model,
            ServiceId = Dalle3Model,
            Size = GetImageDimensions(size),
            Quality = quality == DallE3Quality.Standard ? "standard" : "hd"
        };

        return (await textToImageService.GetImageContentsAsync(new TextContent(prompt), executionSettings))[0].Uri!.ToString();

        static (int Width, int Height) GetImageDimensions(DallE3ImageSize size) => size switch
        {
            DallE3ImageSize.Square1024 => (1024, 1024),
            DallE3ImageSize.Landscape1792 => (1792, 1024),
            DallE3ImageSize.Portrait1792 => (1024, 1792),
            _ => throw new ArgumentOutOfRangeException(nameof(size))
        };
    }
}
