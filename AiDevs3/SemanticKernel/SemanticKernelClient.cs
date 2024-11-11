using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.AudioToText;

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
        double temperature = 0.2)
    {
        var kernel = _kernelFactory.BuildSemanticKernel();
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ModelId = model,
            ServiceId = aiProvider.ToString(),
            MaxTokens = maxTokens,
            Temperature = temperature
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
