using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using AiDevs3.Tasks.S04E01___Interfejs.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs3.Tasks.S04E01___Interfejs;

public class S04E01 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S04E01> _logger;
    private readonly Kernel _kernel;

    public S04E01(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        Kernel kernel,
        ILogger<S04E01> logger,
        ILoggerFactory loggerFactory,
        IFunctionInvocationFilter functionFilter,
        IAutoFunctionInvocationFilter autoFunctionInvocationFilter,
        IPromptRenderFilter promptFilter) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
        _kernel = kernel;

        // Add photo operation plugins with their respective loggers
        _kernel.Plugins.AddFromObject(new RepairPhotoPlugin(ApiKey, CentralaBaseUrl, _semanticKernelClient,
            loggerFactory.CreateLogger<RepairPhotoPlugin>()));
        _kernel.Plugins.AddFromObject(new DarkenPhotoPlugin(ApiKey, CentralaBaseUrl, _semanticKernelClient,
            loggerFactory.CreateLogger<DarkenPhotoPlugin>()));
        _kernel.Plugins.AddFromObject(new BrightenPhotoPlugin(ApiKey, CentralaBaseUrl, _semanticKernelClient,
            loggerFactory.CreateLogger<BrightenPhotoPlugin>()));
        _kernel.Plugins.AddFromObject(new AnalyzePhotoPlugin(_semanticKernelClient, kernel, httpClient,
            loggerFactory.CreateLogger<AnalyzePhotoPlugin>()));
        _kernel.FunctionInvocationFilters.Add(functionFilter);
        _kernel.AutoFunctionInvocationFilters.Add(autoFunctionInvocationFilter);
        _kernel.PromptRenderFilters.Add(promptFilter);
    }

    protected override string LessonName => "S04E01 — Interfejs";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("Starting photo analysis conversation");

        var startResponse = await SubmitResults("photos", "START");
        _logger.LogInformation("Initial response: {Response}", startResponse);

        // Extract photo URLs from the response
        var photoUrls = await ExtractPhotoUrls(startResponse);
        _logger.LogInformation("Extracted {Count} photo URLs", photoUrls.Count);

        var allAnalyses = new List<string>();
        foreach (var photoUrl in photoUrls)
        {
            var analysis = await ProcessSinglePhoto(photoUrl);
            allAnalyses.Add(analysis);
        }

        // Generate final description based on all analyses
        var finalDescriptionPrompt = $"""
                                      Based on the following photo analyses, create a detailed Polish language description (rysopis) of a person:
                                      <descriptions>
                                      {string.Join("\n\n", allAnalyses)}
                                      </descriptions>
        
                                      Extract the most important features and characteristics a person from all provided descriptions.
                                      Make it formal, precise, and focused on permanent physical characteristics.
                                      If some information is inconsistent or unclear, use the most common or repeated details.
                                      Use Polish language
                                      Avoid mentioning temporary features like clothing unless they appear consistently across photos.
                                      """;
        
        var finalDescription = await _semanticKernelClient.ExecutePrompt(
            ModelConfiguration.Gpt4o_Mini_202407,
            null,
            finalDescriptionPrompt,
            maxTokens: 1000);
        
        _logger.LogInformation("Generated final description: {Description}", finalDescription);
        
        var response = await SubmitResults("photos", finalDescription);
        return TypedResults.Json(response);
    };

    private async Task<List<string>> ExtractPhotoUrls(string response)
    {
        var extractPrompt = $"""
                             From this text response:
                             {response}

                             1. Extract the base URL (e.g., https://example.com/photos/)
                             2. Extract all image filenames (e.g., photo1.jpg)
                             3. Combine them to create full URLs

                             Return the complete URLs, one per line, in this format:
                             https://example.com/photos/photo1.jpg
                             https://example.com/photos/photo2.jpg
                             etc.

                             Return only the complete URLs, nothing else.
                             """;

        var urlsText = await _semanticKernelClient.ExecutePrompt(
            ModelConfiguration.Gpt4o_202408,
            null,
            extractPrompt,
            maxTokens: 500);

        return urlsText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(url => url.StartsWith("http"))
            .ToList();
    }

    private async Task<string> ProcessSinglePhoto(string photoUrl)
    {
        _logger.LogInformation("Processing photo: {Url}", photoUrl);

        var prompt = $"""
                      Analyze this photo: {photoUrl}

                      Important: If you cannot clearly identify or analyze the person in the photo, use the available correction tools to improve image quality before analysis:
                      - repair_photo for removing noise and glitches
                      - brighten_photo for dark images
                      - darken_photo for overexposed (too bright) images

                      Follow these steps:
                      1. Create an optimized version of the URL by adding "-small" before the extension
                      2. Evaluate image quality for identification:
                         - If features are unclear or hard to distinguish, apply appropriate corrections
                         - after each correction, re-evaluate the image quality and check if the person is clearly visible
                         - if the person is still not clearly visible, apply additional corrections
                         - if the person is clearly visible, proceed to the next step
                      3. After achieving the best possible image quality, analyze the photo and provide a detailed description of the person
                      4. It may happen that analyzed image does not contain a person. In such case return only 'No person detected' message

                      Remember: Don't hesitate to use multiple correction tools if needed to get the clearest possible view of the person.
                      """;

        var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments(
            new OpenAIPromptExecutionSettings
            {
                ServiceId = ModelConfiguration.Gpt4o_202408.CreateServiceId(),
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            }));

        var stringResult = result.GetValue<string>() ?? "No result";

        _logger.LogInformation("Generated description: {Description}", stringResult);

        return stringResult;
    }
}
