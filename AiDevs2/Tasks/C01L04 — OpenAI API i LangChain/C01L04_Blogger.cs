using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

public class C01L04_Blogger : Lesson
{
    protected override string LessonName => "C01L04 — OpenAI API i LangChain - blogger";
    protected override string TaskName => "blogger";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClient);
        var answer = await GetBlogPosts(task.Blog!, configuration);

        await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, answer, httpClient);
        return answer;
    };

    private static async Task<string> GetBlogPosts(List<string> input, IConfiguration configuration)
    {
        var kernel = BuildSemanticKernel(configuration);

        const string SystemMessage = """
                                     <message role=""system"">You're a proffesional cook who writes their own blog. You will be provided with an array of chapter names that you need to write a blog post about.
                                     Provide two sentences for each provided chapter name. Chapter names will be provided in the following format:
                                     ```
                                     "Wstęp: kilka słów na temat historii pizzy"
                                     "Niezbędne składniki na pizzę"
                                     "Robienie pizzy"
                                     ```

                                     As a response I want a json array  with the following structure - don't inlcude without separators (###):
                                     ###
                                     [
                                         "<Two sentences about the history of pizza>",
                                         "<Two sentences about the necessary ingredients for pizza>",
                                         "<Two sentences about making pizza>",
                                     ]
                                     ###
                                     The output should be a valid json array
                                     </message>

                                     """;

        var chapterNames = string.Join("\n", input);
        var userPrompt = $"""
                          <message role=""user"">
                          {chapterNames}
                          </message>
                          """;

        var prompt = $"{SystemMessage}\n{userPrompt}";
        var blogChapters = kernel.CreateFunctionFromPrompt(prompt, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 500 });

        var completion = await kernel.InvokeAsync(blogChapters);
        var value = completion.GetValue<string>();
        return value!;
    }
}
