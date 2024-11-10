using AiDevs3.DependencyInjection;
using AiDevs3.SemanticKernel;
using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSimpleConsole();
});

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

builder.RegisterModules(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
