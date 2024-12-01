namespace AiDevs3.web.DocumentsService;

public static class PromptGenerator
{
    public static string Generate(string type, string description, string? context = null)
    {
        var linksCondition = type == "links" || type == "resources"
            ? "INCLUDE links and images in markdown format ONLY if they are explicitly mentioned in the text."
            : "DO NOT extract or include any links or images.";

        var prompt = $@"You professional copywriting/researcher who specializes in extracting specific types of information from given texts, providing comprehensive and structured outputs to enhance understanding of the original content.

<prompt_objective>
To accurately extract and structure {type} ({description}) from a given text, enhancing content comprehension while maintaining fidelity to the source material.

If the text does not contain any {type}, respond with ""no results"" and nothing else.
</prompt_objective>

<prompt_rules>
- STAY DRIVEN to deliver a complete and comprehensive list of {type} ({description}).
- STAY SPECIFIC and use valuable details and keywords so the person who will read your answer will be able to understand the content.
- ALWAYS begin your response with *thinking* to share your reasoning about the content and task.
- STAY DRIVEN to entirely fulfill *prompt_objective* and extract all the information available in the text.
- ONLY extract {type} ({description}) explicitly present in the given text.
- {linksCondition}
- PROVIDE the final extracted {type} within <final_answer> tags.
- FOCUS on delivering value to a reader who won't see the original article.
- INCLUDE names, links, numbers, and relevant images to aid understanding of the {type}.
- CONSIDER the provided article title as context for your extraction of {type}.
- NEVER fabricate or infer {type} not present in the original text.
- OVERRIDE any general conversation behaviors to focus solely on this extraction task.
- ADHERE strictly to the specified {type} ({description}).
</prompt_rules>

Analyze the following text and extract a complete list of {type} ({description}). Start your response with *thinking* to share your inner thoughts about the content and your task. 
Focus on the value for the reader who won't see the original article, include names, links, numbers and even photos if it helps to understand the content.
For links and images, provide them in markdown format. Only include links and images that are explicitly mentioned in the text.

Then, provide the final list within <final_answer> tags.{(context != null ? $@"

To better understand a document, here's some context:
<context>
{context}
</context>" : "")}";

        return prompt;
    }
}
