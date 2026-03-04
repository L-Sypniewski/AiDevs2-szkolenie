using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace AiDevs4.Tools.UrlFetcher;

public class UrlFetcherTool
{
    private readonly HttpClient _httpClient;

    public UrlFetcherTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [Description("Fetches the text content from a URL via an HTTP GET request.")]
    public async Task<string> FetchUrl(
        [Description("The URL to fetch data from.")] string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return $"Error: '{url}' is not a valid HTTP/HTTPS URL.";
        }

        try
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (HttpRequestException ex)
        {
            return $"Error fetching URL '{url}': {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            return $"Error: Request to '{url}' timed out.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [Description("Downloads a file from a URL and saves it to a specified local path.")]
    public async Task<string> DownloadFile(
        [Description("The URL of the file to download.")] string url,
        [Description("The local file path where the downloaded file should be saved.")] string destinationPath)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return $"Error: '{url}' is not a valid HTTP/HTTPS URL.";
        }

        var fullDestination = Path.GetFullPath(destinationPath);

        try
        {
            var directory = Path.GetDirectoryName(fullDestination);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            await using var fileStream = File.Create(fullDestination);
            await response.Content.CopyToAsync(fileStream);

            var fileInfo = new FileInfo(fullDestination);
            return $"Successfully downloaded file to '{fullDestination}' ({fileInfo.Length} bytes).";
        }
        catch (HttpRequestException ex)
        {
            return $"Error downloading from '{url}': {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            return $"Error: Download from '{url}' timed out.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public IList<AITool> CreateTools() =>
    [
        AIFunctionFactory.Create(FetchUrl),
        AIFunctionFactory.Create(DownloadFile)
    ];
}
