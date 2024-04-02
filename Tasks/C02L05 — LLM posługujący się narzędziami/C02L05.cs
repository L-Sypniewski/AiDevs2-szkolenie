using Microsoft.AspNetCore.Mvc;

namespace AiDevs2_szkolenie.Tasks;

public class C02L05 : Lesson
{
    protected override string LessonName => "C02L05 — LLM posługujący się narzędziami";
    protected override string TaskName => "functions";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClient);
        const string Schema = """
                              {
                                  "name": "addUser",
                                  "description": "Add a user with name, surname, and year of birth",
                                  "parameters": {
                                      "type": "object",
                                      "properties": {
                                          "name": {
                                              "type": "string",
                                              "description": "Name of the user"
                                          },
                                          "surname": {
                                              "type": "string",
                                              "description": "Surname of the user"
                                          },
                                          "year": {
                                              "type": "integer",
                                              "description": "Year of birth of the user"
                                          }
                                      }
                                  }
                              }
                              """;
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, answer: Schema, httpClient);
        return new { task.Question, Answer = Schema, AnswerResponse = answerResponse };
    };
}
