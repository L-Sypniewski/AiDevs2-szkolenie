using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

public class C03L05 : Lesson
{
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

    protected override string LessonName => "C03L05 — Produkcyjne zastosowanie modeli OpenAI";
    protected override string TaskName => "people";

    private readonly string _peopleJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "people.json");

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C03L05> logger,
        [FromServices] ILoggerFactory loggerFactory,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (_, task) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());
        var kernel = BuildSemanticKernel(configuration);
        var question = task.Question;

        var people = await LoadPeople(_peopleJsonPath, configuration, httpClientFactory.CreateClient());
        var fullName = await GetBasicFullNameForm(question, BuildSemanticKernel(configuration));

        if (!people.TryGetValue(fullName, out var person))
        {
            return new { Question = question, Answer = person?.ToString() ?? "", AnswerResponse = "" };
        }

        if (person.PlaceOfResidence is null)
        {
            person = await GetPersonWithFetchedData(kernel, person);
            people[fullName] = person;
            await SavePeopleToDisk(people, _peopleJsonPath);
        }

        var answer = await AnswerQuestion(person, question, kernel);
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), await GetToken(configuration, httpClientFactory.CreateClient()),
            JsonSerializer.Serialize(answer), httpClientFactory.CreateClient());

        return new { Question = question, Answer = answer, AnswerResponse = answerResponse };
    };

    private static async ValueTask<Dictionary<string, Person>> LoadPeople(string jsonFilepath, IConfiguration configuration, HttpClient httpClient)
    {
        if (File.Exists(jsonFilepath))
        {
            return LoadPeopleFromDisk(jsonFilepath);
        }

        var people = (await httpClient.GetFromJsonAsync<Person[]>($"{configuration.GetValue<Uri>("AiDevsBaseUrl")}/data/people.json"))!;
        var dictionaryFromList = people.ToDictionary(p => $"{p.FirstName} {p.LastName}", p => p);
        await SavePeopleToDisk(dictionaryFromList, jsonFilepath);
        return dictionaryFromList;
    }

    private static Dictionary<string, Person> LoadPeopleFromDisk(string path)
    {
        var peopleJson = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Dictionary<string, Person>>(peopleJson)!;
    }

    private static async Task SavePeopleToDisk(Dictionary<string, Person> people, string path)
    {
        var peopleJson = JsonSerializer.Serialize(people);
        await File.WriteAllTextAsync(path, peopleJson);
    }

    private static async Task<string> GetBasicFullNameForm(string question, Kernel kernel)
    {
        var prompt = $"""
                      You will be provided a sentence in Polish. Your task is to provide the basic, non-diminutive form of the name of the person mentioned in the sentence. Do not correct or change the spelling of the names, including diacritics or perceived misspellings. Return only the basic form of the name exactly as it appears in the question.
                                                           ###Example 1
                                                           Q: Jakie jest ulubione jedzenie Roberta Lewandowskiego?
                                                           A: Robert Lewandowski
                                                           ###
                                                           ###Example 2
                                                           Q: Gdzie wczoraj była Ania Kowalska?
                                                           A: Anna Kowalska
                                                           ###
                      The names are correct as given; do not attempt to modify them.
                      Think twice, because some of the surnames have only one form, do not try to change the form if it's not necessary

                      Q: {question}
                      """;
        var answerFunction =
            kernel.CreateFunctionFromPrompt(prompt, new OpenAIPromptExecutionSettings { MaxTokens = 10, Temperature = 0.2, ChatSystemPrompt = "" });
        var response = await kernel.InvokeAsync<string>(answerFunction);

        return response!;
    }

    private static async Task<Person> GetPersonWithFetchedData(Kernel kernel, Person person)
    {
        var info = await GetInfoAboutPerson(person, kernel);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(info)!;
        person = person with
        {
            PlaceOfResidence = jsonElement.GetProperty("placeOfResidence").GetString(),
            FavouriteFood = jsonElement.GetProperty("favouriteFood").GetString()
        };
        return person;
    }

    private static async Task<string> GetInfoAboutPerson(Person person, Kernel kernel)
    {
        var prompt = $$"""
                       You will be provided a sentence about a person in Polish. I need you to extract and provide the following information about the person:
                       - The person's place of residence
                       - The person's favorite food
                       as a JSON object with the following structure:
                       {
                           "placeOfResidence": "<basic form of the place of residence>",
                           "favouriteFood": "<basic form of the favourite food>"
                       }

                       Q: {{person.Info}}
                       """;
        var answerFunction =
            kernel.CreateFunctionFromPrompt(prompt, new OpenAIPromptExecutionSettings { MaxTokens = 50, Temperature = 0.2, ChatSystemPrompt = "" });
        var response = await kernel.InvokeAsync<string>(answerFunction);

        return response!;
    }

    private static async Task<string> AnswerQuestion(Person person, string question, Kernel kernel)
    {
        const string Prompt = """
                              You will be provided a question about a person in Polish. Question is about one of the following:
                              - The person's place of residence
                              - The person's favorite food
                              - The person's favorite color

                              Your task is to provide the answer to the question based on the information you have about the person.
                              The answer should be in Polish and should be concise.
                              Answer only using the context below:
                              ###Context
                              Favorite food: {{$favouriteFood}}
                              Favorite color: {{$favouriteColor}}
                              Favorite place: {{$placeOfResidence}}
                              ####
                              Q: {{$question}}
                              """;

        var kernelArguments = new KernelArguments()
        {
            ["question"] = question,
            ["favouriteFood"] = person.FavouriteFood,
            ["favouriteColor"] = person.FavouriteColor,
            ["placeOfResidence"] = person.PlaceOfResidence
        };
        var answerFunction =
            kernel.CreateFunctionFromPrompt(Prompt, new OpenAIPromptExecutionSettings { MaxTokens = 10, Temperature = 0.2, ChatSystemPrompt = "" });
        var response = await kernel.InvokeAsync<string>(answerFunction, kernelArguments);
        return response!;
    }
}
