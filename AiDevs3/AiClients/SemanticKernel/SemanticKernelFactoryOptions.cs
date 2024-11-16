using System.ComponentModel.DataAnnotations;

namespace AiDevs3.AiClients.SemanticKernel;

public record AiOptions
{
    public const string ConfigName = "Ai";
    public AiProviderSettings OpenAi { get; init; } = null!;
    public AiProviderSettings GithubModels { get; init; } = null!;

}

public record AiProviderSettings
{
    /// <summary>
    /// Use this property to specify the API endpoint for the OpenAI compatible local API.
    /// </summary>
    public Uri? ApiEndpoint { get; init; }

    [Required]
    public required string ApiKey { get; init; } = null!;
}
