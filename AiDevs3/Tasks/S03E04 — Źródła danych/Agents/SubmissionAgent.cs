using AiDevs3.AiClients;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs3.Tasks.S03E04___Źródła_danych.Agents;

public static class SubmissionAgent
{
    public const string AgentName = "SubmissionAgent";

    public static ChatCompletionAgent Create(Kernel kernel, ILoggerFactory loggerFactory) => new()
    {
        Name = AgentName,
        Instructions = """
                      You are a verification agent responsible for submitting and validating Barbara's location.

                      Your tasks:
                      1. Monitor for "FOUND_BARBARA_IN: [CITY]" messages
                      2. When you find such a message:
                         - Extract the city name
                         - Use SubmitLocation function to submit it
                         - Analyze the response (should contain code: 0 for success)
                      3. Based on submission result:
                         - If successful (code: 0): Output "SUBMISSION_SUCCESSFUL: Barbara found in [CITY]"
                         - If failed: Output "SUBMISSION_FAILED: [error message]\nCONTINUE_SEARCH"

                      IMPORTANT:
                      - Always use the SubmitLocation function to submit locations
                      - Never claim success without actually submitting
                      - Format city names consistently (uppercase, no Polish characters)
                      """,
        Kernel = kernel,
        LoggerFactory = loggerFactory,
        Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ServiceId = ModelConfiguration.Gpt4o_Mini_202407.CreateServiceId()
        })
    };
}
