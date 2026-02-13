namespace AiDevs4.AiClients;

public enum AiProvider
{
    OpenAI,
    GithubModels,
    Ollama
}

public enum ModelConfiguration
{
    // OpenAI API (3 models)
    Gpt5,
    Gpt5_Mini,
    Gpt4_1,

    // GitHub Models (7 models)
    Gpt4_1_Github,
    Gpt4_1_Mini_Github,
    Gpt5_Mini_Github,
    Gpt5_Nano_Github,
    O3_Mini_Github,
    O4_Mini_Github,
    Phi4_Github,

    // Ollama (3 models)
    OllamaLlama31,
    OllamaDeepSeekR1,
    OllamaPhi4
}

public static class ModelConfigurations
{
    private static readonly Dictionary<ModelConfiguration, (string ModelId, AiProvider Provider)> s_modelMappings = new()
    {
        // OpenAI API
        { ModelConfiguration.Gpt5, ("gpt-5", AiProvider.OpenAI) },
        { ModelConfiguration.Gpt5_Mini, ("gpt-5-mini", AiProvider.OpenAI) },
        { ModelConfiguration.Gpt4_1, ("gpt-4.1", AiProvider.OpenAI) },

        // GitHub Models
        { ModelConfiguration.Gpt4_1_Github, ("gpt-4.1", AiProvider.GithubModels) },
        { ModelConfiguration.Gpt4_1_Mini_Github, ("gpt-4.1-mini", AiProvider.GithubModels) },
        { ModelConfiguration.Gpt5_Nano_Github, ("gpt-5-nano", AiProvider.GithubModels) },
        { ModelConfiguration.O3_Mini_Github, ("o3-mini", AiProvider.GithubModels) },
        { ModelConfiguration.O4_Mini_Github, ("o4-mini", AiProvider.GithubModels) },
        { ModelConfiguration.Phi4_Github, ("phi-4", AiProvider.GithubModels) },

        // Ollama
        { ModelConfiguration.OllamaLlama31, ("llama3.1", AiProvider.Ollama) },
        { ModelConfiguration.OllamaDeepSeekR1, ("deepseek-r1", AiProvider.Ollama) },
        { ModelConfiguration.OllamaPhi4, ("phi4", AiProvider.Ollama) }
    };

    public static string GetModelId(this ModelConfiguration config) => s_modelMappings[config].ModelId;
    public static AiProvider GetProvider(this ModelConfiguration config) => s_modelMappings[config].Provider;
    public static string CreateServiceId(this ModelConfiguration config) => $"{config.GetModelId()}-{config.GetProvider()}";
}
