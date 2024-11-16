using HtmlAgilityPack;

namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public static class HtmlProcessor
{
    public static Article ArticleFromHtml(string html, Uri baseArticleUrl, bool isForMediaProcessing = false)
    {
        var doc = CleanedHtml(html);

        var paragraphs = new List<Paragraph>();
        var paragraphNodes = doc.DocumentNode.SelectNodes("//p").Where(p => !p.InnerHtml.Contains("<figure>") || isForMediaProcessing).ToList();

        paragraphs.AddRange(paragraphNodes.Select(pNode => new
        {
            pNode,
            headerTitle = FindNearestHeader(pNode) ?? "No header"
        })
            .Select(@t => new Paragraph
            {
                Text = @t.pNode.InnerText.Trim(),
                HeaderTitle = @t.headerTitle
            }));

        var images = new List<ImageContent>();
        var figures = doc.DocumentNode.SelectNodes("//figure")?.ToList();

        if (figures == null)
        {
            return new Article
            {
                Paragraphs = paragraphs,
                Images = images,
                Audio = []
            };
        }

        foreach (var figure in figures)
        {
            var img = figure.SelectSingleNode(".//img");
            var figcaption = figure.SelectSingleNode(".//figcaption");

            if (img == null)
            {
                continue;
            }

            var srcAttr = img.GetAttributeValue("src", img.GetAttributeValue("data-cfsrc", ""));
            var imageUrl = new Uri(Path.Combine(baseArticleUrl.AbsoluteUri, srcAttr));
            var caption = figcaption?.InnerText.Trim() ?? "";

            images.Add(new ImageContent
            {
                Caption = caption,
                Url = imageUrl,
            });
        }

        var audio = new List<AudioContent>();
        var audioElements = doc.DocumentNode.SelectNodes("//audio")?.ToList();

        if (audioElements == null)
        {
            return new Article
            {
                Paragraphs = paragraphs,
                Images = images,
                Audio = audio
            };
        }

        foreach (var audioElement in audioElements)
        {
            var source = audioElement.SelectSingleNode(".//source");
            if (source == null)
            {
                continue;
            }

            var srcAttr = source.GetAttributeValue("src", "");
            var audioUrl = new Uri(Path.Combine(baseArticleUrl.AbsoluteUri, srcAttr));

            var context = GetSurroundingParagraphs(audioElement, paragraphs, 2);

            audio.Add(new AudioContent
            {
                Url = audioUrl,
                Context = context
            });
        }

        return new Article
        {
            Paragraphs = paragraphs,
            Images = images,
            Audio = audio
        };
    }

    public static HtmlDocument CleanedHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Remove HTML comments only
        var comments = doc.DocumentNode.SelectNodes("//comment()")?.ToList();
        if (comments != null)
        {
            foreach (var comment in comments)
            {
                comment.Remove();
            }
        }

        // Remove head section
        var head = doc.DocumentNode.SelectSingleNode("//head");
        head?.Remove();

        // Remove scripts
        var scripts = doc.DocumentNode.SelectNodes("//script")?.ToList();
        if (scripts != null)
        {
            foreach (var script in scripts)
            {
                script.Remove();
            }
        }

        // Remove styles
        var styles = doc.DocumentNode.SelectNodes("//style")?.ToList();
        if (styles != null)
        {
            foreach (var style in styles)
            {
                style.Remove();
            }
        }

        return doc;
    }

    private static List<Paragraph> GetSurroundingParagraphs(HtmlNode figureNode, List<Paragraph> allParagraphs, int count, bool isForMediaProcessing = false)
    {
        var doc = figureNode.OwnerDocument;
        var allParagraphNodes = doc.DocumentNode
                                    .SelectNodes("/html/body//p")
                                    ?
                                    .Where(p => !p.InnerHtml.Contains("<figure>") || isForMediaProcessing)
                                    .ToList()
                                ?? [];

        var paragraphsBeforeFigure = allParagraphNodes
            .Count(p => p.StreamPosition < figureNode.StreamPosition);

        var startIndex = Math.Max(0, paragraphsBeforeFigure - count);
        var endIndex = Math.Min(allParagraphs.Count, paragraphsBeforeFigure + count);

        return allParagraphs.GetRange(startIndex, Math.Min(count * 2, endIndex - startIndex));
    }

    private static string? FindNearestHeader(HtmlNode node)
    {
        var current = node;
        while (current != null)
        {
            var previousSibling = current.PreviousSibling;
            while (previousSibling != null)
            {
                if (previousSibling.Name.StartsWith("h") && previousSibling.Name.Length == 2)
                {
                    var headerNum = previousSibling.Name[1];
                    if (char.IsDigit(headerNum) && headerNum >= '1' && headerNum <= '6')
                    {
                        return previousSibling.InnerText.Trim();
                    }
                }

                previousSibling = previousSibling.PreviousSibling;
            }

            current = current.ParentNode;
        }

        return null;
    }

    public static string ExtractTextContent(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.InnerText;
    }
}
