using AiDevs3.AiClients;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs3.Tasks.S03E04___Źródła_danych.Agents;

public static class ReasoningAgent
{
    public const string AgentName = "ReasoningAgent";

    public static ChatCompletionAgent Create(Kernel kernel, ILoggerFactory loggerFactory) =>
        new()
        {
            Name = AgentName,
            Instructions = """
                           You are an intelligence analyst focused on tracking Barbara's movements.

                           Your specific tasks:
                           1. Analyze information from the SearchAgent:
                              - Focus on connections between people and places
                              - Identify patterns in Barbara's movements
                              - Track who Barbara has met and where

                           2. Make conclusions:
                              - When you're confident about Barbara's location
                              - Explain your reasoning briefly
                              - Respond with "FOUND_BARBARA_IN: [CITY]" when location is confirmed

                           3. Request more searches:
                              - Ask SearchAgent to look up specific people or places
                              - Prioritize leads that could reveal Barbara's current location
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
