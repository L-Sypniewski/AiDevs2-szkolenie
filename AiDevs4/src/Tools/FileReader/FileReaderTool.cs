using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace AiDevs4.Tools.FileReader;

public class FileReaderTool
{
    [Description("Reads the full text content of a file at the given path.")]
    public static string ReadTextFile(
        [Description("The full path to the text file to read.")] string filePath)
    {
        if (!File.Exists(filePath))
        {
            return $"Error: File not found: {filePath}";
        }

        try
        {
            var content = File.ReadAllText(filePath);
            return content;
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    [Description("Reads a specified number of lines from the beginning of a text file.")]
    public static string ReadLines(
        [Description("The full path to the text file to read.")] string filePath,
        [Description("The maximum number of lines to read from the beginning of the file.")] int maxLines = 100)
    {
        if (!File.Exists(filePath))
        {
            return $"Error: File not found: {filePath}";
        }

        try
        {
            var lines = File.ReadLines(filePath).Take(maxLines).ToList();
            return $"Read {lines.Count} line(s):{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    [Description("Lists files in a directory, optionally filtering by a search pattern.")]
    public static string ListFiles(
        [Description("The directory path to list files from.")] string directoryPath,
        [Description("A search pattern to filter files (e.g. '*.txt', '*.json'). Defaults to '*' for all files.")] string searchPattern = "*",
        [Description("Whether to search subdirectories recursively.")] bool recursive = false)
    {
        if (!Directory.Exists(directoryPath))
        {
            return $"Error: Directory not found: {directoryPath}";
        }

        try
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
            var fileList = files.Select(f => Path.GetRelativePath(directoryPath, f));

            return $"Found {files.Length} file(s):{Environment.NewLine}{string.Join(Environment.NewLine, fileList)}";
        }
        catch (Exception ex)
        {
            return $"Error listing files: {ex.Message}";
        }
    }

    public static IList<AITool> CreateTools() =>
    [
        AIFunctionFactory.Create(ReadTextFile),
        AIFunctionFactory.Create(ReadLines),
        AIFunctionFactory.Create(ListFiles)
    ];
}
