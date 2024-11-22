var builder = DistributedApplication.CreateBuilder(args);

var qdrant = builder.AddQdrant("qdrant")
    .WithOtlpExporter()
    .WithHttpEndpoint(port: 33267, targetPort: 6333, name: "dashboard", isProxied: false)
    .WithDataVolume("rag3")
    .WithLifetime(ContainerLifetime.Persistent);
var ollama = builder.AddOllama("ollama")
    // .WithContainerRuntimeArgs("--gpus=all")
    // .WithOpenWebUI()
    .WithDataVolume();

var embeddingModel = ollama.WithOtlpExporter().AddModel("ollama-embeddings", "mxbai-embed-large");
var phiModel = ollama.WithOtlpExporter().AddModel("ollama-phi", "phi3:mini"); 

builder.AddProject<Projects.AiDevs3>("AiDevs3")
    .WithReference(qdrant)
    .WithReference(embeddingModel)
    .WithReference(phiModel);

builder.Build().Run();
