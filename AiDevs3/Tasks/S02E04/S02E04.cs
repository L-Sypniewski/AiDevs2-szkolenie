using System.IO.Compression;
using System.Text.Json.Serialization;
using AiDevs3.SemanticKernel;
using Microsoft.Extensions.Logging;

namespace AiDevs3.Tasks.S02E04;

public class S02E04 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S02E04> _logger;

    public S02E04(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S02E04> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S02E04 — Połączenie wielu formatów";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("Starting S02E04 task");

        var zipPath = await DownloadZipFile();
        var extractPath = Path.Combine(Path.GetTempPath(), "factory_files");

        ExtractZipFile(zipPath, extractPath);

        var (txtPaths, pngPaths, mp3Paths) = GetCategorizedFilePaths(extractPath, _logger);

        // Cleanup
        Directory.Delete(extractPath, true);
        File.Delete(zipPath);

        return TypedResults.Ok(new
        {
            message = "Files processed successfully",
            counts = new
            {
                txt = txtPaths.Length,
                png = pngPaths.Length,
                mp3 = mp3Paths.Length
            }
        });
    };

    private async Task<string> DownloadZipFile()
    {
        var zipUrl = $"{CentralaBaseUrl}/dane/pliki_z_fabryki.zip";
        var zipPath = Path.Combine(Path.GetTempPath(), "factory_files.zip");

        _logger.LogInformation("Downloading ZIP file from {Url}", zipUrl);

        using var response = await HttpClient.GetAsync(zipUrl);
        using var fileStream = File.Create(zipPath);
        await response.Content.CopyToAsync(fileStream);

        return zipPath;
    }

    private void ExtractZipFile(string zipPath, string extractPath)
    {
        _logger.LogInformation("Extracting ZIP file to {Path}", extractPath);

        if (Directory.Exists(extractPath))
        {
            Directory.Delete(extractPath, true);
        }

        Directory.CreateDirectory(extractPath);
        ZipFile.ExtractToDirectory(zipPath, extractPath);
    }

    private static (string[] txtPaths, string[] pngPaths, string[] mp3Paths) GetCategorizedFilePaths(string directoryPath, ILogger<S02E04> logger)
    {
        var txtPaths = Directory.GetFiles(directoryPath, "*.txt", SearchOption.TopDirectoryOnly);
        logger.LogInformation("Found .txt files: {TxtPaths}", string.Join(", ", txtPaths));

        var pngPaths = Directory.GetFiles(directoryPath, "*.png", SearchOption.TopDirectoryOnly);
        logger.LogInformation("Found .png files: {PngPaths}", string.Join(", ", pngPaths));

        var mp3Paths = Directory.GetFiles(directoryPath, "*.mp3", SearchOption.TopDirectoryOnly);
        logger.LogInformation("Found .mp3 files: {Mp3Paths}", string.Join(", ", mp3Paths));

        return (txtPaths, pngPaths, mp3Paths);
    }
}
