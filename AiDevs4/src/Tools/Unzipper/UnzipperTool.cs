using System.ComponentModel;
using System.IO.Compression;
using Microsoft.Extensions.AI;

namespace AiDevs4.Tools.Unzipper;

public class UnzipperTool
{
    [Description("Extracts a ZIP archive to a specified directory and returns the list of extracted file paths.")]
    public static string ExtractZip(
        [Description("The full path to the ZIP file to extract.")] string zipFilePath,
        [Description("The directory where the contents should be extracted to. If not specified, extracts to a directory next to the ZIP file with the same name.")] string? destinationDirectory = null)
    {
        if (!File.Exists(zipFilePath))
        {
            return $"Error: File not found: {zipFilePath}";
        }

        destinationDirectory ??= Path.Combine(
            Path.GetDirectoryName(zipFilePath) ?? ".",
            Path.GetFileNameWithoutExtension(zipFilePath));

        var fullDestination = Path.GetFullPath(destinationDirectory);

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in archive.Entries)
            {
                var entryDestination = Path.GetFullPath(Path.Combine(fullDestination, entry.FullName));
                if (!entryDestination.StartsWith(fullDestination + Path.DirectorySeparatorChar) &&
                    entryDestination != fullDestination)
                {
                    return $"Error: ZIP archive contains an entry with a path traversal attempt: '{entry.FullName}'.";
                }
            }

            ZipFile.ExtractToDirectory(zipFilePath, fullDestination, overwriteFiles: true);

            var extractedFiles = Directory.GetFiles(fullDestination, "*", SearchOption.AllDirectories);
            var fileList = string.Join(Environment.NewLine, extractedFiles.Select(f => Path.GetRelativePath(fullDestination, f)));

            return $"Successfully extracted {extractedFiles.Length} file(s) to '{fullDestination}':{Environment.NewLine}{fileList}";
        }
        catch (InvalidDataException)
        {
            return $"Error: '{zipFilePath}' is not a valid ZIP archive.";
        }
        catch (Exception ex)
        {
            return $"Error extracting ZIP: {ex.Message}";
        }
    }

    [Description("Lists the contents of a ZIP archive without extracting it.")]
    public static string ListZipContents(
        [Description("The full path to the ZIP file to inspect.")] string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            return $"Error: File not found: {zipFilePath}";
        }

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            var entries = archive.Entries
                .Select(e => $"{e.FullName} ({e.Length} bytes)")
                .ToList();

            return $"ZIP archive contains {entries.Count} {(entries.Count == 1 ? "entry" : "entries")}:{Environment.NewLine}{string.Join(Environment.NewLine, entries)}";
        }
        catch (InvalidDataException)
        {
            return $"Error: '{zipFilePath}' is not a valid ZIP archive.";
        }
        catch (Exception ex)
        {
            return $"Error reading ZIP: {ex.Message}";
        }
    }

    public static IList<AITool> CreateTools() =>
    [
        AIFunctionFactory.Create(ExtractZip),
        AIFunctionFactory.Create(ListZipContents)
    ];
}
