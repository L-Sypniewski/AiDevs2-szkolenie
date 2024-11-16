namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public static class AudioProcessingPrompts
{
    public const string AudioTranscriptionSystemMessage =
        """
        Transcribe the provided audio content accurately, maintaining the original meaning and context.
        <prompt_objective>
        To produce a precise transcription of the audio content that captures all spoken words, maintaining proper punctuation and paragraph structure.
        </prompt_objective>
        <prompt_rules>
        - TRANSCRIBE all spoken content accurately
        - MAINTAIN proper sentence structure and punctuation
        - PRESERVE speaker transitions if multiple speakers are present
        - INDICATE any unclear or inaudible portions with [inaudible]
        - DO NOT add interpretations or additional context
        - CAPTURE non-verbal audio cues only if crucial to meaning
        - Text will be provided in Polish and should be returned in Polish
        </prompt_rules>
        Provide an accurate transcription that faithfully represents the audio content.
        """;

    public const string AudioContextSummarySystemMessage =
        """
        Create a concise summary of the audio context that captures key information and main points.
        <prompt_objective>
        To generate a clear and focused summary of the audio context that highlights essential information and maintains logical flow.
        </prompt_objective>
        <prompt_rules>
        - IDENTIFY main topics and key points
        - MAINTAIN chronological or logical order of information
        - FOCUS on factual content and important details
        - EXCLUDE redundant or peripheral information
        - PRESERVE the original meaning and intent
        - ENSURE the summary is self-contained and coherent
        - KEEP the language clear and professional
        - Text will be provided in Polish and should be returned in Polish
        </prompt_rules>
        Create a comprehensive yet concise summary that effectively communicates the core content.
        """;

    public const string CombineAudioDescriptionSystemMessage =
        """
        Create a unified description combining audio transcription and context summary.
        <prompt_objective>
        To produce a cohesive narrative that effectively merges the audio content with its contextual information, highlighting relationships and key insights.
        </prompt_objective>
        <prompt_rules>
        - INTEGRATE audio transcription and context seamlessly
        - HIGHLIGHT relationships between audio content and context
        - IDENTIFY and emphasize key themes and connections
        - MAINTAIN chronological or logical flow
        - ENSURE consistency between both sources
        - PRESERVE important details from both inputs
        - RESOLVE any contradictions or inconsistencies
        - DO NOT add interpretations beyond provided content
        - Text will be provided in Polish and should be returned in Polish
        </prompt_rules>
        Create a comprehensive description that effectively combines and contextualizes both sources of information.
        """;
}
