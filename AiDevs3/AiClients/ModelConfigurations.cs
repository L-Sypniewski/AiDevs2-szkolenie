namespace AiDevs3.AiClients;

public enum AiProvider
{
    OpenAI,
    GithubModels
}

public enum ModelConfiguration
{
    Gpt4o_202408 = 0,
    Gpt4o_Mini_202407,
    Whisper1,
    Gpt4_Turbo_202404,
    O1_Mini_202409,
    O1_Preview_202409,
    Gpt4o_Github,
    Gpt4o_Mini_Github,
    O1_Mini_Github,
    O1_Preview_Github,
    Phi35_Mini_Instruct,
    Phi35_MoE_Instruct,
    Phi35_Vision_Instruct,
    Dalle3
}

public static class ModelConfigurations
{
    private static readonly Dictionary<ModelConfiguration, (string ModelId, AiProvider Provider)> s_modelMappings = new()
    {
        { ModelConfiguration.Gpt4o_202408, ("gpt-4o-2024-08-06", AiProvider.OpenAI) },
        { ModelConfiguration.Gpt4o_Mini_202407, ("gpt-4o-mini-2024-07-18", AiProvider.OpenAI) },
        { ModelConfiguration.Whisper1, ("whisper-1", AiProvider.OpenAI) },
        { ModelConfiguration.Gpt4_Turbo_202404, ("gpt-4-turbo-2024-04-09", AiProvider.OpenAI) },
        { ModelConfiguration.O1_Mini_202409, ("o1-mini-2024-09-12", AiProvider.OpenAI) },
        { ModelConfiguration.O1_Preview_202409, ("o1-preview-2024-09-12", AiProvider.OpenAI) },
        { ModelConfiguration.Gpt4o_Github, ("gpt-4o", AiProvider.GithubModels) },
        { ModelConfiguration.Gpt4o_Mini_Github, ("gpt-4o-mini", AiProvider.GithubModels) },
        { ModelConfiguration.O1_Mini_Github, ("o1-mini", AiProvider.GithubModels) },
        { ModelConfiguration.O1_Preview_Github, ("o1-preview", AiProvider.GithubModels) },
        { ModelConfiguration.Phi35_Mini_Instruct, ("Phi-3.5-mini-instruct", AiProvider.GithubModels) },
        { ModelConfiguration.Phi35_MoE_Instruct, ("Phi-3.5-MoE-instruct", AiProvider.GithubModels) },
        { ModelConfiguration.Phi35_Vision_Instruct, ("Phi-3.5-vision-instruct", AiProvider.GithubModels) },
        { ModelConfiguration.Dalle3, ("dall-e-3", AiProvider.OpenAI) }
    };

    public static string GetModelId(this ModelConfiguration config) => s_modelMappings[config].ModelId;
    public static AiProvider GetProvider(this ModelConfiguration config) => s_modelMappings[config].Provider;
    public static string CreateServiceId(this ModelConfiguration config) => $"{config.GetModelId()}-{config.GetProvider()}";
    public static bool IsValidConfiguration(string modelId, AiProvider provider) => s_modelMappings.Values.Contains((modelId, provider));
}