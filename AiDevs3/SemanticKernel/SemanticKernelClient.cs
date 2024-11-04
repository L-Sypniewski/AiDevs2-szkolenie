using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs3.SemanticKernel;

public class SemanticKernelClient
{
    private readonly SemanticKernelFactory _kernelFactory;

    public SemanticKernelClient(SemanticKernelFactory semanticKernelFactory) => _kernelFactory = semanticKernelFactory;


    public async Task<string> ExecutePrompt(string model, string systemPrompt, string userPrompt, double temperature = 0.2)
    {
        var kernel = _kernelFactory.BuildSemanticKernel(model);
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = temperature
        };
        var kernelArguments = new KernelArguments(promptExecutionSettings)
        {
            ["system"] = systemPrompt,
            ["question"] = userPrompt
        };
        const string SystemMessage =
            "<message role=\"system\">{{$system}}</message>";
        const string UserMessage =
            "<message role=\"user\">{{$question}}\n</message>";
        const string Prompt = $"{SystemMessage}\n{UserMessage}";

        // TODO: Measure prompt cache hit rate: https://platform.openai.com/docs/guides/prompt-caching
        //TODO: Optimizing LLMs for accuracy: https://platform.openai.com/docs/guides/optimizing-llm-accuracy
        return (await kernel.InvokePromptAsync<string>(Prompt, kernelArguments))!;
    }
}
