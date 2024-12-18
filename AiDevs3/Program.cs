using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using AiDevs3.DependencyInjection;
using AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;
using AiDevs3.Tasks.S03E02___Wyszukiwanie_Semantyczne;
using AiDevs3.Tasks.S04E01___Interfejs;
using AiDevs3.Tasks.S04E05___Mechaniki_obsługi_narzędzi;
using AiDevs3.Utils;
using AiDevs3.web.DocumentsService;
using AiDevs3.web.TextService;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.SemanticKernel;
using NorthernNerds.Aspire.Neo4j;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// Add Qdrant client and vector store
builder.AddQdrantClient("qdrant");
builder.Services.AddQdrantVectorStore();
builder.Services.AddQdrantVectorStoreRecordCollection<Guid, ArticleRag>(ArticleRag.CollectionName);
builder.Services.AddQdrantVectorStoreRecordCollection<Guid, WeaponsTestsRag>(WeaponsTestsRag.CollectionName);
builder.Services.AddQdrantVectorStoreRecordCollection<Guid, RafalsNotesRag>(RafalsNotesRag.CollectionName);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<HtmlConverter>();

// Add OllamaSharp
builder.AddOllamaSharpEmbeddingGenerator("ollama-embeddings");
builder.AddOllamaSharpChatClient("ollama-phi");

builder.AddNeo4jClient("graph-db");

builder.Services.AddAiClients(builder.Configuration);

builder.Services.AddS02E05Dependencies();
builder.Services.AddS03E02Dependencies();

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

// Add services from web folder
builder.Services.AddTransient<ITextService, TextService>();
builder.Services.AddTransient<IDocumentService, DocumentService>();

// Add RafalsNotesProcessor registration
builder.Services.AddTransient<RafalsNotesProcessor>();

// Add these two lines to register the filters
builder.Services.AddSingleton<IFunctionInvocationFilter, MyFunctionFilter>();
builder.Services.AddSingleton<IPromptRenderFilter, MyPromptFilter>();
builder.Services.AddSingleton<IAutoFunctionInvocationFilter, FunctionInvocationFilter>();

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
