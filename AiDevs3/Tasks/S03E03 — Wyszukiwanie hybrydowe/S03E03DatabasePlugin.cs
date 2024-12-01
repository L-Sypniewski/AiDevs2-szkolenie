using System.ComponentModel;
using System.Text.Json;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S03E03___Wyszukiwanie_hybrydowe;

public class S03E03DatabasePlugin
{
    private const ModelConfiguration LlmModel = ModelConfiguration.Phi35_MoE_Instruct;
    private const double LlmTemperature = 0.2;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly string _apiUrl;
    private readonly ILogger<S03E03DatabasePlugin> _logger;

    public S03E03DatabasePlugin(
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        IConfiguration configuration,
        ILogger<S03E03DatabasePlugin> logger)
    {
        _httpClient = httpClient;
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
        var baseUrl = configuration["CentralaBaseUrl"] ?? throw new InvalidOperationException("CentralaBaseUrl not configured");
        _apiUrl = $"{baseUrl}/apidb";
        _apiKey = configuration["AiDevsApiKey"] ?? throw new InvalidOperationException("AiDevsApiKey not configured");
    }


    [KernelFunction, Description("Analyzes database schema by executing show tables and show create table commands")]
    public async Task<string> AnalyzeSchema()
    {
        // Get list of tables
        var tablesJson = await ExecuteQuery("show tables");
        var tables = await ExtractTableNames(tablesJson);

        // Get schema for each table
        var schemaResponses = new List<string>();
        foreach (var table in tables)
        {
            var schemaJson = await ExecuteQuery($"show create table {table}");
            var schema = await ExtractSchema(schemaJson, table);
            schemaResponses.Add($"""
                                 <table_schema name="{table}">
                                 {schema}
                                 </table_schema>
                                 """);
        }

        return string.Join("\n\n", schemaResponses);
    }

    private async Task<string> ExtractSchema(string jsonResponse, string tableName)
    {
        const string SystemPrompt = """
                                    You are a SQL expert. Extract the CREATE TABLE statement from the JSON response.
                                    Clean up and format the schema definition. Include only essential information about table structure.
                                    Return only the cleaned CREATE TABLE statement without any additional text or JSON formatting.
                                    Focus on:
                                    1. Table name
                                    2. Column names and types
                                    3. Primary keys
                                    4. Foreign keys
                                    5. Important constraints
                                    6. Any information needed to query the table
                                    """;

        _logger.LogInformation("Extracting schema for table {TableName}. Input JSON: {JsonResponse}", tableName, jsonResponse);
        var result = await _semanticKernelClient.ExecutePrompt(
            LlmModel,
            systemPrompt: null,
            $"{SystemPrompt}\n{jsonResponse}",
            maxTokens: 500,
            temperature: LlmTemperature);

        _logger.LogInformation("LLM Schema extraction response: {Result}", result);
        return result.Trim();
    }

    private async Task<string[]> ExtractTableNames(string jsonResponse)
    {
        const string SystemPrompt = """
                                    You are a JSON parser with SQL knowledge. Extract SQL database table names from the provided JSON response.
                                    Return only table names as a comma-separated list without any additional text.
                                    <Example_Output>
                                    users, orders, products
                                    </Example_Output>
                                    """;

        _logger.LogInformation("Extracting table names from JSON: {JsonResponse}", jsonResponse);
        var result = await _semanticKernelClient.ExecutePrompt(
            LlmModel,
            systemPrompt: null,
            $"{SystemPrompt}\n{jsonResponse}",
            maxTokens: 100,
            temperature: LlmTemperature);

        _logger.LogInformation("LLM Table names extraction response: {Result}", result);
        return [.. result.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    [KernelFunction, Description("Generates and executes SQL query based on schema and requirements")]
    public async Task<string> ExecuteSqlQuery([Description("SQL query to execute")] string query)
    {
        return await ExecuteQuery(query);
    }

    [KernelFunction, Description("Generates SQL query based on schema and requirements")]
    public async Task<string> GenerateQuery(
        [Description("Database schema information")] string schema,
        [Description("Query requirements in natural language")]
        string requirements)
    {
        const string SystemPrompt = """
                                    You are a SQL expert. Generate a SQL query based on the provided schema and requirements.
                                    Follow these rules:
                                    1. Return ONLY the SQL query without any explanations or additional text
                                    2. Use proper table joins if needed
                                    3. Ensure the query is optimized and correct
                                    4. Use only tables and columns that exist in the schema
                                    5. Don't wrap the query in quotes or other characters, return the raw SQL query
                                    """;

        var userPrompt = $"""
                          Schema:
                          {schema}

                          Requirements:
                          {requirements}
                          """;

        _logger.LogInformation("Generating SQL query. Schema: {Schema}, Requirements: {Requirements}", schema, requirements);
        var result = await _semanticKernelClient.ExecutePrompt(
            LlmModel,
            systemPrompt: null,
            $"{SystemPrompt}\n{userPrompt}",
            maxTokens: 500,
            temperature: LlmTemperature);

        _logger.LogInformation("LLM Generated SQL query: {Result}", result);
        return result;
    }

    private async Task<string> ExecuteQuery(string query)
    {
        _logger.LogInformation("Executing SQL query: {Query}", query);
        var request = new
        {
            task = "database",
            apikey = _apiKey,
            query
        };

        var response = await _httpClient.PostAsJsonAsync(_apiUrl, request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
        _logger.LogInformation("Query execution response: {Result}", result?.RootElement.GetRawText());

        return result?.RootElement.GetRawText() ?? string.Empty;
    }
}
