using System.ComponentModel.DataAnnotations;

namespace AiDevs4.AiClients;

public record AiOptions
{
    public const string ConfigName = "Ai";
    public AiProviderSettings OpenAi { get; init; } = null!;
    public AiProviderSettings GithubModels { get; init; } = null!;
    public OllamaProviderSettings? Ollama { get; init; }
}

public record AiProviderSettings
{
    public Uri? ApiEndpoint { get; init; }

    [Required]
    public required string ApiKey { get; init; } = null!;
}

public record OllamaProviderSettings
{
    [Required]
    public required Uri Endpoint { get; init; } = null!;
}
