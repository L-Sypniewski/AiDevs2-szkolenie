var builder = DistributedApplication.CreateBuilder(args);

// Add GitHub Models API key as a secret parameter
var githubModelsApiKey = builder.AddParameter("github-models-apikey", secret: true);

/* // Add Qdrant vector database with persistent storage and dashboard
var qdrant = builder.AddQdrant("qdrant")
    .WithOtlpExporter()
    .WithHttpEndpoint(port: 33267, targetPort: 6333, name: "dashboard", isProxied: false)
    .WithDataVolume("aidevs4-rag")
    .WithLifetime(ContainerLifetime.Persistent);

// Add Ollama for local LLM inference
var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// Add embedding model for vector operations
var embeddingModel = ollama
    .WithOtlpExporter()
    .AddModel("ollama-embeddings", "mxbai-embed-large");

// Add Phi-3 model for local inference
var phiModel = ollama
    .WithOtlpExporter()
    .AddModel("ollama-phi", "phi3:mini"); */

// Add the AiDevs4 API project with GitHub Models and service references
builder.AddProject<Projects.AiDevs4>("aidevs4")
    .WithEnvironment("Ai__GithubModels__ApiKey", githubModelsApiKey)
/*     .WithReference(qdrant)
    .WithReference(embeddingModel)
    .WithReference(phiModel) */
    .WithExternalHttpEndpoints();

builder.Build().Run();
