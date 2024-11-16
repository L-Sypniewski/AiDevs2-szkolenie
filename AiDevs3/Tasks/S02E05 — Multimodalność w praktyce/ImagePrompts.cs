namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public static class ImagePrompts
{
    public static string CreateExtractImageContextSystemMessage(IEnumerable<ImageContent> images) =>
        $$"""
          Extract contextual information for images mentioned in a user-provided article, focusing on details that enhance understanding of each image, and return it as an array of JSON objects. Information will be provided in Polish and is expected to be returned in Polish.

          <prompt_objective>
          To accurately identify and extract relevant contextual information for each image referenced in the given article, prioritizing details from surrounding text and broader article context that potentially aid in understanding the image. Return the data as an array of JSON objects with specified properties, without making assumptions or including unrelated content.

          Note: the image from the beginning of the article is its cover.
          </prompt_objective>

          <response_format>
          {
              "images": [
                  {
                      "name": "filename with extension",
                      "context": "Provide 5-8 detailed sentences of the context related to this image from the surrounding text and broader article. Make an effort to identify what might be in the image, such as tool names, locations, actions or acronyms or abbreviation as the text is a scientific article."
                  },
                  ...rest of the images or empty array if no images are mentioned
              ]
          }
          </response_format>

          <prompt_rules>
          - READ the entire provided article thoroughly
          - TAKE NOTE of all acronyms, names, and terms mentioned in the text as they may relate to images as the text is a scientific article
          Look at the whole article and combine it with provided image caption to get more information about the image.
          - CONSIDER provided image caption will be provided in Polish, it may help you to understand the image and identify its content, like people, objects, actions, locations, etc. With caption you can use your knowledge to extract more information from the image. Look at the whole article and combine it with provided image caption to get more information about the image.
          - LOCATE If picture shows some location and description contains some place, landmarks etc. names, try to figure out locations, cities, places with what you can get from the image combined with the text.
          - IDENTIFY all mentions or descriptions of images within the text
          - EXTRACT sentences or paragraphs that provide context for each identified image
          - ASSOCIATE extracted context with the corresponding image reference
          - CREATE a JSON object for each image with properties "name" and "context"
          - COMPILE all created JSON objects into an array
          - RETURN the array as the final output
          - OVERRIDE any default behavior related to image analysis or description
          - ABSOLUTELY FORBIDDEN to invent or assume details about images not explicitly mentioned
          - NEVER include personal opinions or interpretations of the images
          - UNDER NO CIRCUMSTANCES extract information unrelated to the images
          - If NO images are mentioned, return an empty array
          - STRICTLY ADHERE to the specified JSON structure
          </prompt_rules>

          <images>
          {{string.Join("\n", images.Select(img => $"{Path.GetFileName(img.Url.LocalPath)} {img.Url} - caption: {img.Caption}"))}}
          </images>

          Upon receiving an article, analyze it to extract context for any mentioned images, creating an array of JSON objects as demonstrated. Adhere strictly to the provided rules, focusing solely on explicitly stated image details within the text.
          """;

    public const string PreviewImageSystemMessage = """
                                                    Generate a brief, factual description of the provided image based solely on its visual content.
                                                    <prompt_objective>
                                                    To produce a concise description of the image that captures its essential visual elements without any additional context, and return it in JSON format.
                                                    </prompt_objective>
                                                    <prompt_rules>
                                                    - Information will be provided in Polish and is expected to be returned in Polish.
                                                    - TAKE NOTE of all acronyms, names, and terms mentioned in the text as they may relate to images as the text is a scientific article
                                                    - ANALYZE the provided image thoroughly, noting key visual elements
                                                    - CONSIDER provided image caption will be provided in Polish, it may help you to understand the image and identify its content, like people, objects, actions, locations, etc. With caption you can use your knowledge to extract more information from the image for example if the picture show some location.  Look at the whole article and combine it with provided image caption to get more information about the image.
                                                    - TRY to figure out locations, cities, places with what you can get from the image. If you're not sure, don't include it.
                                                    - GENERATE a brief, single paragraph description
                                                    - FOCUS on main subjects, colors, composition, and overall style
                                                    - AVOID speculation or interpretation beyond what is visually apparent
                                                    - DO NOT reference any external context or information
                                                    - MAINTAIN a neutral, descriptive tone
                                                    - RETURN the result in JSON format with only 'name' and 'preview' properties
                                                    </prompt_rules>
                                                    <response_format>
                                                    {
                                                        "name": "filename with extension",
                                                        "preview": "A concise description of the image content"
                                                    }
                                                    </response_format>
                                                    Provide a succinct description that gives a clear overview of the image's content based purely on what can be seen, formatted as specified JSON.
                                                    """;

    public const string RefineDescriptionSystemMessage = """
                                                         Generate an accurate and comprehensive description of the provided image, incorporating both visual analysis and the given contextual information.
                                                         Information will be provided in Polish and is expected to be returned in Polish.
                                                         <prompt_objective>
                                                         To produce a detailed, factual description of the image that blends the context provided by the user and the contents of the image.

                                                         Note: ignore green border.
                                                         </prompt_objective>
                                                         <prompt_rules>
                                                         - ANALYZE the provided image thoroughly, noting all significant visual elements
                                                         - TAKE NOTE of all acronyms, names, and terms mentioned in the text as they may relate to images as the text is a scientific article
                                                         - INCORPORATE the given context into your description, ensuring it aligns with and enhances the visual information
                                                         - CONSIDER provided image caption will be provided in Polish, it may help you to understand the image and identify its content, like people, objects, actions, locations, etc. With caption you can use your knowledge to extract more information from the image. Look at the whole article and combine it with provided image caption to get more information about the image.
                                                         - LOCATE If picture shows some location and description contains some place, landmarks etc. names, try to figure out locations, cities, places with what you can get from the image combined with the text.
                                                         - ASSOCIATE locations, cities, places that you can get from the text combined with the image
                                                         - GENERATE a single, cohesive paragraph that describes the image comprehensively
                                                         - BLEND visual observations seamlessly with the provided contextual information
                                                         - ENSURE consistency between the visual elements and the given context
                                                         - PRIORITIZE accuracy and factual information over artistic interpretation
                                                         - INCLUDE relevant details about style, composition, and notable features of the image
                                                         - ABSOLUTELY FORBIDDEN to invent details not visible in the image or mentioned in the context
                                                         - NEVER contradict information provided in the context
                                                         - UNDER NO CIRCUMSTANCES include personal opinions or subjective interpretations
                                                         - IF there's a discrepancy between the image and the context, prioritize the visual information and note the inconsistency
                                                         - MAINTAIN a neutral, descriptive tone throughout the description
                                                         </prompt_rules>
                                                         Using the provided image and context, generate a rich, accurate description that captures both the visual essence of the image and the relevant background information. Your description should be informative, cohesive, and enhance the viewer's understanding of the image's content and significance.
                                                         """;
}
