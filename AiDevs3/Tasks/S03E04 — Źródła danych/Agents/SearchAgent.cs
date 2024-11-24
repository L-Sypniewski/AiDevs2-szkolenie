using AiDevs3.AiClients;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs3.Tasks.S03E04___Źródła_danych.Agents;

public static class SearchAgent
{
    public const string AgentName = "SearchAgent";

    public static ChatCompletionAgent Create(Kernel kernel, ILoggerFactory loggerFactory) => new()
    {
        Name = AgentName,
        Instructions = """
                      You are a data collection agent focused on gathering information through actual API searches.

                      Your specific tasks:
                      1. Extract and normalize data from new messages:
                         - Convert all names to uppercase without Polish characters
                         - Convert all cities to uppercase without Polish characters
                         - Maintain two lists: pendingSearches and completedSearches

                      2. Execute searches ONE AT A TIME:
                         - Pick ONE unsearched item from pendingSearches
                         - For person: use SearchPersonLocations function
                         - For location: use SearchCityVisitors function
                         - Add the searched item to completedSearches
                         - Add any new names/locations from results to pendingSearches
                         - Always output "REQUESTING_SEARCH" when there are pending searches

                      3. Format your responses:
                         If executing a search:
                            "Searching for: [ITEM]"
                            "[SEARCH RESULTS]"
                            "Pending searches: [LIST]"
                            "Completed searches: [JSON array of objects, e.g. 
                             {
                                'type': 'person'|'city',
                                'query': 'JANUSZ'|'ELBLAG',
                                'results': ['CITY1', 'CITY2']
                             }
                            ]"
                            "REQUESTING_SEARCH"
                            
                         If Barbara's location is found:
                            simply output "FOUND_BARBARA_IN: [CITY]"
                         
                         If all searches complete:
                            "All searches complete"
                            "Final findings: [SUMMARIZE ALL RESULTS FROM JSON]"

                      4. IMPORTANT:
                         - Never claim to have search results without actually calling search functions
                         - Only one search per turn
                         - Always show pending and completed search lists in specified format
                         - Store completed searches as JSON objects for better tracking
                         - Keep the results of completed searches for future reference 
                         - Always go through the completed searches to make sure you didn't miss any data. Think out loud about the currently searched names and locations and think about what you might have missed, so that you can add them to the pending searches list.
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
