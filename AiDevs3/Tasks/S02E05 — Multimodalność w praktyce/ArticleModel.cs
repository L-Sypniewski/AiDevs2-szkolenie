namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public record Article
{
    public required List<Paragraph> Paragraphs { get; init; } = [];
    public required List<ImageContent> Images { get; init; } = [];
    public required List<AudioContent> Audio { get; init; } = [];

    public string AllParagraphs => string.Join("\n\n",
        Paragraphs
            .Select((p, i) => (p, i))
            .GroupBy(x => x.p.HeaderTitle)
            .OrderBy(g => g.Min(x => x.i))
            .Select(g => $"{g.Key}\n{string.Join("\n", g.Select(x => x.p.Text))}"));
}

public record Paragraph
{
    public required string Text { get; init; }
    public required string HeaderTitle { get; init; }
}

public record ImageContent
{
    public required string Caption { get; init; }
    public required Uri Url { get; init; }
    public string Filename => Url.Segments.Last();
}

public record AudioContent
{
    public required Uri Url { get; init; }
    public string Filename => Url.Segments.Last();
    public required List<Paragraph> Context { get; init; }
}
