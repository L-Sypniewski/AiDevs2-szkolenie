using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiDevs3.SemanticKernel;

public class SemanticKernelClient
{
    private readonly SemanticKernelFactory _kernelFactory;

    public SemanticKernelClient(SemanticKernelFactory semanticKernelFactory) => _kernelFactory = semanticKernelFactory;

    public async Task<string> ExecutePrompt(
        string model,
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
            ServiceId = model,
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
        string? systemPrompt,
        string userPrompt,
        IReadOnlyCollection<ReadOnlyMemory<byte>> imageData,
        int maxTokens,
        double temperature = 0.2)
    {
        var kernel = _kernelFactory.BuildSemanticKernel();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>(serviceKey: model);

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
        string filename,
        Stream audioStream,
        string language = "pl",
        string? prompt = null,
        float temperature = 0.0f)
    {
        var kernel = _kernelFactory.BuildSemanticKernel();
        var audioToTextService = kernel.GetRequiredService<IAudioToTextService>();

        var executionSettings = new OpenAIAudioToTextExecutionSettings(filename)
        {
            Language = language,
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
}
