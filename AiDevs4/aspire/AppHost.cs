using Aspire.Hosting.GitHub;

var builder = DistributedApplication.CreateBuilder(args);

// ==================== OpenAI Provider ====================
var openaiApiKey = builder.AddParameter("openai-apikey", secret: true);
var openai = builder.AddOpenAI("openai")
    .WithApiKey(openaiApiKey);

var openaiGpt5 = openai.AddModel("openai-gpt5", "gpt-5");
var openaiGpt5Mini = openai.AddModel("openai-gpt5-mini", "gpt-5-mini");
var openaiGpt41 = openai.AddModel("openai-gpt41", "gpt-4.1");

// ==================== GitHub Models Provider ====================
var githubApiKey = builder.AddParameter("github-models-apikey", secret: true);

var githubGpt41 = builder.AddGitHubModel("github-gpt41", GitHubModel.OpenAI.OpenAIGPT41)
    .WithApiKey(githubApiKey);
var githubGpt41Mini = builder.AddGitHubModel("github-gpt41-mini", GitHubModel.OpenAI.OpenAIGPT41Mini)
    .WithApiKey(githubApiKey);
var githubGpt5Mini = builder.AddGitHubModel("github-gpt5-mini", GitHubModel.OpenAI.OpenAIGpt5Mini)
    .WithApiKey(githubApiKey);
var githubGpt5Nano = builder.AddGitHubModel("github-gpt5-nano", GitHubModel.OpenAI.OpenAIGpt5Nano)
    .WithApiKey(githubApiKey);
var githubO3Mini = builder.AddGitHubModel("github-o3-mini", GitHubModel.OpenAI.OpenAIO3Mini)
    .WithApiKey(githubApiKey);
var githubO4Mini = builder.AddGitHubModel("github-o4-mini", GitHubModel.OpenAI.OpenAIO4Mini)
    .WithApiKey(githubApiKey);
var githubPhi4 = builder.AddGitHubModel("github-phi4", GitHubModel.Microsoft.Phi4)
    .WithApiKey(githubApiKey);

// ==================== Ollama Provider (requires Docker) ====================
var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var ollamaLlama31 = ollama.AddModel("ollama-llama31", "llama3.1");
var ollamaDeepSeekR1 = ollama.AddModel("ollama-deepseek-r1", "deepseek-r1");
var ollamaPhi4 = ollama.AddModel("ollama-phi4", "phi4");

// ==================== Qdrant (Optional - currently commented out) ====================
/* // Add Qdrant vector database with persistent storage and dashboard
var qdrant = builder.AddQdrant("qdrant")
    .WithOtlpExporter()
    .WithHttpEndpoint(port: 33267, targetPort: 6333, name: "dashboard", isProxied: false)
    .WithDataVolume("aidevs4-rag")
    .WithLifetime(ContainerLifetime.Persistent);
*/

// ==================== Main Project ====================
builder.AddProject<Projects.AiDevs4>("aidevs4")
    // OpenAI references
    .WithReference(openaiGpt5)
    .WithReference(openaiGpt5Mini)
    .WithReference(openaiGpt41)
    // GitHub Models references
    .WithReference(githubGpt41)
    .WithReference(githubGpt41Mini)
    .WithReference(githubGpt5Mini)
    .WithReference(githubGpt5Nano)
    .WithReference(githubO3Mini)
    .WithReference(githubO4Mini)
    .WithReference(githubPhi4)
    // Ollama references
    .WithReference(ollamaLlama31)
    .WithReference(ollamaDeepSeekR1)
    .WithReference(ollamaPhi4)
    // Qdrant reference (uncomment if Qdrant is enabled above)
    // .WithReference(qdrant)
    .WithExternalHttpEndpoints();

builder.Build().Run();
