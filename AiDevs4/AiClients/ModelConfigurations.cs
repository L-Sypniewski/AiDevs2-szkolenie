namespace AiDevs4.AiClients;

public enum AiProvider
{
    OpenAI,
    GithubModels,
    Ollama
}

public enum ModelConfiguration
{
    Gpt4o_202411,
    Gpt4o_Mini_202407,
    Gpt4o_Github,
    Gpt4o_Mini_Github,
    OllamaPhi
}

public static class ModelConfigurations
{
    private static readonly Dictionary<ModelConfiguration, (string ModelId, AiProvider Provider)> s_modelMappings = new()
    {
        { ModelConfiguration.Gpt4o_202411, ("gpt-4o-2024-11-20", AiProvider.OpenAI) },
        { ModelConfiguration.Gpt4o_Mini_202407, ("gpt-4o-mini-2024-07-18", AiProvider.OpenAI) },
        { ModelConfiguration.Gpt4o_Github, ("gpt-4o", AiProvider.GithubModels) },
        { ModelConfiguration.Gpt4o_Mini_Github, ("gpt-4o-mini", AiProvider.GithubModels) },
        { ModelConfiguration.OllamaPhi, ("phi3:mini", AiProvider.Ollama) }
    };

    public static string GetModelId(this ModelConfiguration config) => s_modelMappings[config].ModelId;
    public static AiProvider GetProvider(this ModelConfiguration config) => s_modelMappings[config].Provider;
    public static string CreateServiceId(this ModelConfiguration config) => $"{config.GetModelId()}-{config.GetProvider()}";
}
