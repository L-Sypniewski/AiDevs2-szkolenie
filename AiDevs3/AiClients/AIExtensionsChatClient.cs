using Microsoft.Extensions.AI;

namespace AiDevs3.AiClients;

public class AiExtensionsChatClient
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AiExtensionsChatClient(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

    public async Task<string> ExecutePromptWithIChatClient(
        ModelConfiguration model,
        string? systemPrompt,
        string userPrompt,
        int maxTokens,
        float temperature = 0.2f,
        ChatResponseFormat? responseFormat = null,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var chatClient = scope.ServiceProvider.GetRequiredKeyedService<IChatClient>(model.CreateServiceId());

        var messages = new List<ChatMessage>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        }

        messages.Add(new ChatMessage(ChatRole.User, userPrompt));
        var chatCompletion = await chatClient.CompleteAsync(messages, new ChatOptions()
        {
            ModelId = model.GetModelId(),
            MaxOutputTokens = maxTokens,
            Temperature = temperature,
            ResponseFormat = responseFormat
        }, cancellationToken);

        return chatCompletion.Message.Text!;
    }
}
