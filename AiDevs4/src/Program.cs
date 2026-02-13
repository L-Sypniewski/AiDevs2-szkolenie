using System.Text.Json.Serialization;
using AiDevs4.AiClients;
using AiDevs4.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

builder.Services.AddHttpClient();
builder.Services.AddAiClients(builder.Configuration);

builder.RegisterModules(typeof(Program).Assembly);

var app = builder.Build();

// Map health endpoints for Aspire Dashboard
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "AiDevs4 API"));
}

app.MapEndpoints();

app.Run();
