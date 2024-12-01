namespace AiDevs3.web.DocumentsService;

public enum ExtractionType
{
    Topics,
    Entities,
    Keywords,
    Links,
    Resources,
    Takeaways,
    Context
}

public static class ExtractionTypes
{
    public static readonly IReadOnlyDictionary<ExtractionType, string> Descriptions = new Dictionary<ExtractionType, string>
    {
        [ExtractionType.Topics] = "Main subjects covered in the article. Focus here on the headers and all specific topics discussed in the article.",
        [ExtractionType.Entities] = "Mentioned people, places, or things mentioned in the article. Skip the links and images.",
        [ExtractionType.Keywords] = "Key terms and phrases from the content. You can think of them as hastags that increase searchability of the content for the reader.",
        [ExtractionType.Links] = "Complete list of the links and images mentioned with their 1-sentence description.",
        [ExtractionType.Resources] = "Tools, platforms, resources mentioned in the article. Include context of how the resource can be used, what the problem it solves or any note that helps the reader to understand the context of the resource.",
        [ExtractionType.Takeaways] = "Main points and valuable lessons learned. Focus here on the key takeaways from the article that by themself provide value to the reader (avoid vague and general statements like \"its really important\" but provide specific examples and context). You may also present the takeaway in broader context of the article.",
        [ExtractionType.Context] = "Background information and setting. Focus here on the general context of the article as if you were explaining it to someone who didn't read the article."
    };
}
