using System.Text.RegularExpressions;
using Microsoft.ML.Tokenizers;

namespace AiDevs3;

public class Doc
{
    public required string Text { get; set; }
    public required Metadata Metadata { get; set; }
}

public class Metadata
{
    public required int Tokens { get; set; }
    public required Dictionary<string, List<string>> Headers { get; set; }
    public required List<string> Urls { get; set; }
    public required List<string> Images { get; set; }
}

public class TextSplitter
{
    private readonly Tokenizer _tokenizer;

    private readonly IReadOnlyDictionary<string, int> _specialTokens = new Dictionary<string, int>
    {
        ["<|im_start|>"] = 100264,
        ["<|im_end|>"] = 100265,
        ["<|im_sep|>"] = 100266
    };

    public TextSplitter(string modelName)
    {
        _tokenizer = TiktokenTokenizer.CreateForModel(modelName, extraSpecialTokens: _specialTokens);
    }

    private int CountTokens(string text)
    {
        if (_tokenizer == null)
        {
            throw new Exception("Tokenizer not initialized");
        }

        var formattedContent = FormatForTokenization(text);
        var tokens = _tokenizer.EncodeToTokens(formattedContent, out _);
        return tokens.Count;
    }

    private static string FormatForTokenization(string text)
    {
        return $"<|im_start|>user\n{text}<|im_end|>\n<|im_start|>assistant<|im_end|>";
    }

    public Task<List<Doc>> Split(string text, int limit)
    {
        Console.WriteLine($"Starting split process with limit: {limit} tokens");
        var chunks = new List<Doc>();
        var position = 0;
        var totalLength = text.Length;
        var currentHeaders = new Dictionary<string, List<string>>();

        while (position < totalLength)
        {
            Console.WriteLine($"Processing chunk starting at position: {position}");
            var (chunkText, chunkEnd) = GetChunk(text, position, limit);
            var tokens = CountTokens(chunkText);
            Console.WriteLine($"Chunk tokens: {tokens}");

            var headersInChunk = ExtractHeaders(chunkText);
            UpdateCurrentHeaders(currentHeaders, headersInChunk);

            var (content, urls, images) = ExtractUrlsAndImages(chunkText);

            chunks.Add(new Doc
            {
                Text = content,
                Metadata = new Metadata
                {
                    Tokens = tokens,
                    Headers = new Dictionary<string, List<string>>(currentHeaders),
                    Urls = urls,
                    Images = images
                }
            });

            Console.WriteLine($"Chunk processed. New position: {chunkEnd}");
            position = chunkEnd;
        }

        Console.WriteLine($"Split process completed. Total chunks: {chunks.Count}");
        return Task.FromResult(chunks);
    }

    private (string ChunkText, int ChunkEnd) GetChunk(string text, int start, int limit)
    {
        Console.WriteLine($"Getting chunk starting at {start} with limit {limit}");

        var overhead = CountTokens(FormatForTokenization("")) - CountTokens("");

        var end = Math.Min(start + (int) Math.Floor((text.Length - start) * (double) limit / CountTokens(text[start..])), text.Length);

        var chunkText = text[start..end];
        var tokens = CountTokens(chunkText);

        while (tokens + overhead > limit && end > start)
        {
            Console.WriteLine($"Chunk exceeds limit with {tokens + overhead} tokens. Adjusting end position...");
            end = FindNewChunkEnd(start, end);
            chunkText = text[start..end];
            tokens = CountTokens(chunkText);
        }

        end = AdjustChunkEnd(text, start, end, limit);

        chunkText = text[start..end];
        // tokens = CountTokens(chunkText);
        Console.WriteLine($"Final chunk end: {end}");
        return (chunkText, end);
    }

    private int AdjustChunkEnd(string text, int start, int end, int limit)
    {
        var minChunkTokens = limit * 0.8;

        var nextNewline = text.IndexOf('\n', end);
        var prevNewline = text[..end].LastIndexOf('\n');

        if (nextNewline != -1 && nextNewline < text.Length)
        {
            var extendedEnd = nextNewline + 1;
            var chunkText = text[start..extendedEnd];
            var tokens = CountTokens(chunkText);
            if (tokens <= limit && tokens >= minChunkTokens)
            {
                Console.WriteLine($"Extending chunk to next newline at position {extendedEnd}");
                return extendedEnd;
            }
        }

        if (prevNewline > start)
        {
            var reducedEnd = prevNewline + 1;
            var chunkText = text[start..reducedEnd];
            var tokens = CountTokens(chunkText);
            if (tokens <= limit && tokens >= minChunkTokens)
            {
                Console.WriteLine($"Reducing chunk to previous newline at position {reducedEnd}");
                return reducedEnd;
            }
        }

        return end;
    }

    private static int FindNewChunkEnd(int start, int end)
    {
        var newEnd = end - (int) Math.Floor((end - start) / 10.0);
        if (newEnd <= start)
        {
            newEnd = start + 1;
        }

        return newEnd;
    }

    private static Dictionary<string, List<string>> ExtractHeaders(string text)
    {
        var headers = new Dictionary<string, List<string>>();
        var headerRegex = new Regex(@"(^|\n)(#{1,6})\s+(.*)", RegexOptions.Multiline);
        var matches = headerRegex.Matches(text);

        foreach (Match match in matches)
        {
            var level = match.Groups[2].Length;
            var content = match.Groups[3].Value.Trim();
            var key = $"h{level}";

            if (!headers.TryGetValue(key, out var value))
            {
                value = [];
                headers[key] = value;
            }

            value.Add(content);
        }

        return headers;
    }

    private static void UpdateCurrentHeaders(Dictionary<string, List<string>> current, Dictionary<string, List<string>> extracted)
    {
        for (var level = 1; level <= 6; level++)
        {
            var key = $"h{level}";
            if (!extracted.TryGetValue(key, out var value))
            {
                continue;
            }

            current[key] = value;
            ClearLowerHeaders(current, level);
        }
    }

    private static void ClearLowerHeaders(Dictionary<string, List<string>> headers, int level)
    {
        for (var l = level + 1; l <= 6; l++)
        {
            var key = $"h{l}";
            headers.Remove(key);
        }
    }

    private static (string Content, List<string> Urls, List<string> Images) ExtractUrlsAndImages(string text)
    {
        var urls = new List<string>();
        var images = new List<string>();
        var urlIndex = 0;
        var imageIndex = 0;

        var imageRegex = new Regex(@"!\[([^\]]*)\]\(([^)]+)\)");
        var urlRegex = new Regex(@"\[([^\]]+)\]\(([^)]+)\)");

        var content = imageRegex.Replace(text, match =>
        {
            images.Add(match.Groups[2].Value);
            return $"![{match.Groups[1].Value}]({{$img{imageIndex++}}})";
        });

        content = urlRegex.Replace(content, match =>
        {
            urls.Add(match.Groups[2].Value);
            return $"[{match.Groups[1].Value}]({{$url{urlIndex++}}})";
        });

        return (content, urls, images);
    }
}

// Interface for tokenizer (implementation needed)
public interface ITokenizer
{
    List<int> Encode(string text, List<string> specialTokens);
}

// Factory class for tokenizer (implementation needed)
public static class TokenizerFactory
{
    public static Task<ITokenizer> CreateByModelName(string modelName, Dictionary<string, int> specialTokens)
    {
        // Implementation needed
        throw new NotImplementedException();
    }
}
