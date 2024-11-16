using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using AiDevs3.DependencyInjection;
using AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.SemanticKernel;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// Add Qdrant client and vector store
builder.AddQdrantClient("qdrant");
builder.Services.AddQdrantVectorStore();
builder.Services.AddQdrantVectorStoreRecordCollection<Guid, ArticleRag>(ArticleRag.CollectionName);

// Add OllamaSharp
builder.AddOllamaSharpEmbeddingGenerator("ollama-embeddings");

builder.Services.AddAiClients(builder.Configuration);

builder.Services.AddS02E05Dependencies();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("resilient-client")
    .AddResilienceHandler("pipeline", pipelineBuilder =>
    {
        pipelineBuilder.AddTimeout(TimeSpan.FromMinutes(2));

        pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 5,
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.FromSeconds(3),
            ShouldHandle = exception => ValueTask.FromResult(!exception.Outcome.Result?.IsSuccessStatusCode ?? true)
        });
    });

builder.Services.AddTransient<SemanticKernelClient>();

builder.RegisterModules(typeof(Program).Assembly, builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "AiDevs3 API"));
}

app.UseHttpsRedirection();

app.MapEndpoints(app.Services);
app.MapDefaultEndpoints();

app.Run();
