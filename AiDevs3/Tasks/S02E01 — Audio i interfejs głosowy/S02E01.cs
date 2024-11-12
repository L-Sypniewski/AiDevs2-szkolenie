using System.IO.Compression;
using AiDevs3.SemanticKernel;

namespace AiDevs3.Tasks.S02E01___Audio_i_interfejs_głosowy;

public class S02E01 : Lesson
{
    private readonly ILogger<S02E01> _logger;
    private readonly SemanticKernelClient _semanticKernelClient;

    public S02E01(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S02E01> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S02E01 — Audio i interfejs głosowy";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("Processing answer request");

        var audioFiles = await DownloadAndExtractAudioFiles(HttpClient, CentralaBaseUrl, _logger);
        var transcribedAudioFilesPaths = await TranscribedAudioFilesPaths(audioFiles, _logger);
        var streetName = await AnalyzeTranscriptions(transcribedAudioFilesPaths, _logger);
        _logger.LogInformation("Extracted street name: {StreetName}", streetName);
        var responseContent = await SubmitResults("mp3", streetName);
        _logger.LogInformation("Response content: {ResponseContent}", responseContent);
        return TypedResults.Ok(responseContent);
    };

    private static async Task<List<string>> DownloadAndExtractAudioFiles(
        HttpClient httpClient,
        string centralaBaseUrl,
        ILogger logger)
    {
        const string DataFolderName = "data";
        const string ZipFileName = "przesluchania.zip";

        var basePath = Path.GetDirectoryName(typeof(S02E01).Assembly.Location)!;
        var lessonPath = Path.Combine(basePath, "Tasks", "S02E01");
        var dataPath = Path.Combine(lessonPath, DataFolderName);

        Directory.CreateDirectory(dataPath);

        try
        {
            logger.LogInformation("Downloading audio files from centrala");
            var zipPath = Path.Combine(dataPath, ZipFileName);
            var fileUrl = $"{centralaBaseUrl}/dane/{ZipFileName}";

            logger.LogInformation("Downloading zip file from {Url}", fileUrl);
            if (!File.Exists(zipPath))
            {
                await using var fileStream = File.Create(zipPath);
                var response = await httpClient.GetStreamAsync(fileUrl);
                logger.LogInformation("Saving zip file to {Path}", zipPath);
                await response.CopyToAsync(fileStream);
            }
            else
            {
                logger.LogInformation("Using cached zip file from {Path}", zipPath);
            }

            logger.LogInformation("Extracting audio files from zip");
            ZipFile.ExtractToDirectory(zipPath, dataPath, overwriteFiles: true);

            var audioFiles = Directory.GetFiles(dataPath, "*.m4a", SearchOption.AllDirectories).ToList();
            var concatenatedFileNames = string.Join(", ", audioFiles);
            logger.LogInformation("Found {Count} audio files: {FileNames}", audioFiles.Count, concatenatedFileNames);

            return audioFiles;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while downloading/extracting audio files");
            throw;
        }
    }

    private async Task<List<string>> TranscribedAudioFilesPaths(List<string> audioFiles, ILogger logger)
    {
        logger.LogInformation("Transcribing audio files");
        var transcriptionFiles = new List<string>(audioFiles.Count);

        foreach (var audioFile in audioFiles)
        {
            var transcriptionPath = Path.ChangeExtension(audioFile, ".txt");

            if (File.Exists(transcriptionPath))
            {
                logger.LogInformation("Using existing transcription file: {TranscriptionPath}", transcriptionPath);
            }
            else
            {
                logger.LogInformation("Creating new transcription for file: {AudioFile}", audioFile);
                await using var audioStream = File.OpenRead(audioFile);
                var transcription = await _semanticKernelClient.TranscribeAudioAsync(
                    "whisper-1",
                    Path.GetFileName(audioFile),
                    audioStream,
                    language: "pl");

                await File.WriteAllTextAsync(transcriptionPath, transcription);
                logger.LogInformation("Saved transcription to: {TranscriptionPath}", transcriptionPath);
            }

            transcriptionFiles.Add(transcriptionPath);
        }

        var concatenatedFileNames = string.Join(", ", transcriptionFiles);
        logger.LogInformation("Processed {Count} transcription files: {FileNames}",
            transcriptionFiles.Count, concatenatedFileNames);

        return transcriptionFiles;
    }

    private async Task<string> AnalyzeTranscriptions(List<string> transcriptionsFilePaths, ILogger logger)
    {
        logger.LogInformation("Analyzing transcriptions");

        var combinedTestimonies = string.Join("\n",
            transcriptionsFilePaths.Select((testimony, index) =>
            {
                var fileName = Path.GetFileNameWithoutExtension(transcriptionsFilePaths[index]);
                var witnessName = Path.GetFileNameWithoutExtension(fileName);
                return $"<testimony witness=\"{witnessName}\">\n{testimony}\n</testimony>";
            }));

        logger.LogInformation("Combined testimonies with XML tags and witness names");

        var analysis = await _semanticKernelClient.ExecutePrompt(
            "gpt-4o-2024-08-06",
            TestimonyAnalysisSystemPrompt,
            combinedTestimonies,
            maxTokens: 3000,
            temperature: 0.5);

        logger.LogInformation("Analysis completed: {Analysis}", analysis);

        // Extract street name from <answer> tags
        var match = System.Text.RegularExpressions.Regex.Match(analysis, @"<answer>(.*?)</answer>");
        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find street name in analysis response");
        }

        var streetName = match.Groups[1].Value;
        logger.LogInformation("Extracted street name: {StreetName}", streetName);

        return streetName;
    }

    private const string TestimonyAnalysisSystemPrompt = """
                                                         Role: You are an investigator piecing together information about Professor Andrzej Maj's university institute street. Your task is to determine and clearly tag the specific street name of the institute where he teaches.

                                                         <critical_instructions>
                                                         1. ESSENTIAL: You must think OUT LOUD through your entire reasoning process
                                                         2. CRUCIAL CONTEXT: 
                                                         - The street name is not directly stated in the testimonies
                                                         - You must use contextual clues from testimonies AND your internal knowledge
                                                         - Some testimonies may be contradictory or unusual
                                                         - Rafał's testimony should be given special attention but considered potentially unstable
                                                         - Provided street names must be ignored as they are added to mislead
                                                         3. REQUIRED: Place your final answer inside <answer></answer> tags
                                                         </critical_instructions>

                                                         Think out loud and provide a detailed explanation of your reasoning process. Your answer must be supported by clear evidence and logical connections.

                                                         First figure out the city and institute name, then deduce the street name based on the testimonies.
                                                         As the text comes in Polish conduct reasoning in Polish.

                                                         <output_format>
                                                             <thinking_process>
                                                             .....
                                                             </thinking_process>
                                                             
                                                             <answer>STREET_NAME</answer>
                                                         </output_format>

                                                         Remember: Provided street names must be ignored as they are added to mislead
                                                         """;
}
