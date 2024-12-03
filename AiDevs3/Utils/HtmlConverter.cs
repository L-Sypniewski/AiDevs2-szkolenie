using System.Diagnostics;

namespace AiDevs3.Utils;

public class HtmlConverter
{
    private readonly string _nodeScriptPath;
    private readonly string _nodePath;

    public HtmlConverter(IConfiguration configuration)
    {
        _nodeScriptPath = Path.Combine(AppContext.BaseDirectory, "Utils");
        _nodePath = configuration["NodePath"]
            ?? throw new Exception("NodePath configuration is missing");
    }

    public async Task<string> ConvertToMarkdown(string html)
    {
        var base64Html = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html));

        var startInfo = new ProcessStartInfo
        {
            FileName = _nodePath,
            Arguments = $"convert.js {base64Html}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = _nodeScriptPath
        };

        using var process = Process.Start(startInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return output.Trim();
    }
}

