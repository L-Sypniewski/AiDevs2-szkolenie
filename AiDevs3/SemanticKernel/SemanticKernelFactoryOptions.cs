using System.ComponentModel.DataAnnotations;

namespace AiDevs3.SemanticKernel;

public class SemanticKernelFactoryOptions
{
    public const string ConfigName = "SemanticKernelFactory";

    /// <summary>
    /// Use this property to specify the API endpoint for the OpenAI compatible local API.
    /// </summary>
    public Uri? ApiEndpoint { get; init; }

    [Required]
    public required string ApiKey { get; init; } = null!;
}
