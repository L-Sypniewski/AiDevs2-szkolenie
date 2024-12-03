using UglyToad.PdfPig;
using System.Text;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using System.Text.RegularExpressions;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using System.Text.Json;
using Microsoft.Extensions.VectorData;

namespace AiDevs3.Tasks.S04E05___Mechaniki_obsługi_narzędzi;

public class S04E05 : Lesson
{
    private readonly ILogger<S04E05> _logger;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly IVectorStoreRecordCollection<Guid, RafalsNotesRag> _recordCollection;
    private readonly RafalsNotesProcessor _notesProcessor;

    public S04E05(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<S04E05> logger,
        SemanticKernelClient semanticKernelClient,
        IVectorStoreRecordCollection<Guid, RafalsNotesRag> recordCollection,
        RafalsNotesProcessor notesProcessor) : base(configuration, httpClient)
    {
        _logger = logger;
        _semanticKernelClient = semanticKernelClient;
        _recordCollection = recordCollection;
        _notesProcessor = notesProcessor;
    }

    protected override string LessonName => "S04E05 — Mechaniki obsługi narzędzi";

    protected override Delegate GetAnswerDelegate => async (CancellationToken cancellationToken) =>
    {
        _logger.LogInformation("Starting notes analysis process");

        if (_recordCollection.CollectionExistsAsync(cancellationToken).Result)
        {
            _logger.LogInformation("Collection already exists, skipping creation");
        }
        else
        {
            var pdfBytes = await DownloadPdfFile(cancellationToken);
            var pagesContent = (await ExtractTextFromPdf(pdfBytes)).ToList();

            // Get document-wide summary first
            var documentSummary = await CreateDocumentSummary(pagesContent, cancellationToken);
            _logger.LogInformation("Document summary created. Length: {Length}", documentSummary.Length);

            await _notesProcessor.ProcessPages(pagesContent, documentSummary, cancellationToken);
        }

        var questions = await FetchQuestions(cancellationToken);
        var answers = await ProcessQuestions(questions, cancellationToken);

        var finalResponse = await SubmitResults("notes", answers);
        return TypedResults.Ok(finalResponse);
    };

    private async Task<byte[]> DownloadPdfFile(CancellationToken cancellationToken)
    {
        var pdfUrl = $"{CentralaBaseUrl}/dane/notatnik-rafala.pdf";
        _logger.LogInformation("Downloading PDF file from {Url}", pdfUrl);

        var pdfBytes = await HttpClient.GetByteArrayAsync(pdfUrl, cancellationToken);
        _logger.LogInformation("Successfully downloaded PDF file, size: {Size} bytes", pdfBytes.Length);

        return pdfBytes;
    }

    private async Task<Dictionary<string, string>> FetchQuestions(CancellationToken cancellationToken)
    {
        var questionsUrl = $"{CentralaBaseUrl}/data/{ApiKey}/notes.json";
        _logger.LogInformation("Fetching questions from {Url}", questionsUrl);

        var questionsJson = await HttpClient.GetStringAsync(questionsUrl, cancellationToken);
        var questions = JsonSerializer.Deserialize<Dictionary<string, string>>(questionsJson);
        _logger.LogInformation("Successfully fetched questions: {Questions}", questionsJson);

        if (questions == null)
        {
            _logger.LogError("Failed to deserialize questions JSON");
            throw new Exception("Failed to retrieve questions list");
        }

        _logger.LogInformation("Successfully fetched {Count} questions", questions.Count);
        return questions;
    }

    private async Task<Dictionary<string, string>> ProcessQuestions(
        Dictionary<string, string> questions,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to process {Count} questions", questions.Count);
        var answers = new Dictionary<string, string>();

        foreach (var (questionId, question) in questions)
        {
            _logger.LogInformation("Processing question {Id}: {Question}", questionId, question);

            var searchContext = await _notesProcessor.SearchContent(question, cancellationToken);
            _logger.LogInformation("Found context for question {Id}. Context length: {Length}",
                questionId, searchContext?.Length ?? 0);

            var prompt = BuildAnswerPrompt(searchContext ?? string.Empty, question);

            var answer = await _semanticKernelClient.ExecutePrompt(
                ModelConfiguration.Gpt4o_202411,
                systemPrompt: null,
                userPrompt: prompt,
                maxTokens: 1000,
                temperature: 0.1,
                cancellationToken: cancellationToken);

            var answerMatch = Regex.Match(answer, @"<odpowiedz>(.*?)<\/odpowiedz>", RegexOptions.Singleline);
            answers[questionId] = answerMatch.Success ? answerMatch.Groups[1].Value : "Nie znaleziono odpowiedzi";
            _logger.LogInformation("Generated answer for question {Id}. Answer: {answerMatch}",
                questionId, answers[questionId]);
        }

        _logger.LogInformation("Completed processing all {Count} questions", questions.Count);
        return answers;
    }

    private async Task<IReadOnlyCollection<PageContent>> ExtractTextFromPdf(byte[] pdfBytes)
    {
        _logger.LogInformation("Starting PDF text extraction. Input size: {Size} bytes", pdfBytes.Length);
        var pages = new List<PageContent>();

        using var document = PdfDocument.Open(pdfBytes);
        _logger.LogInformation("PDF document opened. Pages: {PageCount}, Size: {Size} bytes",
            document.NumberOfPages, pdfBytes.Length);

        foreach (var page in document.GetPages())
        {
            _logger.LogInformation("Processing page {PageNumber}/{TotalPages}",
                page.Number, document.NumberOfPages);

            var pageContent = await ProcessPdfPage(page);
            pages.Add(pageContent);

            _logger.LogInformation("Page {PageNumber} processed. Text: {TextLength} chars, Images: {ImageCount}",
                page.Number, pageContent.TextContent.Length, pageContent.ImageDescriptions.Count);
        }

        _logger.LogInformation("PDF extraction completed. Pages: {PageCount}, Total text: {TextLength} chars",
            pages.Count, pages.Sum(p => p.TextContent.Length));
        return pages;
    }

    private async Task<PageContent> ProcessPdfPage(UglyToad.PdfPig.Content.Page page)
    {
        _logger.LogInformation("Starting page {PageNumber} processing", page.Number);

        var text = ExtractTextFromPage(page);
        var imageDescriptions = await ExtractImageDescriptions(page);

        _logger.LogInformation("Page {PageNumber} processing completed. Text: {TextLength} chars, Images: {ImageCount}",
            page.Number, text.Length, imageDescriptions.Count);

        return new PageContent(text, imageDescriptions);
    }

    private string ExtractTextFromPage(UglyToad.PdfPig.Content.Page page)
    {
        _logger.LogInformation("Starting text extraction from page {PageNumber}", page.Number);

        var words = page.GetWords(NearestNeighbourWordExtractor.Instance);
        _logger.LogDebug("Found {WordCount} words on page {PageNumber}", words.Count(), page.Number);

        var pageBuilder = new StringBuilder();
        foreach (var word in words)
        {
            pageBuilder.Append(word.Text).Append(' ');
        }

        var cleanText = CleanupText(pageBuilder.ToString());
        _logger.LogInformation("Text extraction completed for page {PageNumber}. Raw: {RawLength}, Clean: {CleanLength}",
            page.Number, pageBuilder.Length, cleanText.Length);

        return cleanText;
    }

    private async Task<IReadOnlyCollection<(int Index, string Description)>> ExtractImageDescriptions(UglyToad.PdfPig.Content.Page page)
    {
        _logger.LogInformation("Starting image extraction from page {PageNumber}", page.Number);

        var images = page.GetImages().ToArray();
        _logger.LogInformation("Found {ImageCount} images on page {PageNumber}", images.Length, page.Number);

        var descriptions = new List<(int Index, string Description)>();
        if (images.Length <= 1)
        {
            _logger.LogInformation("No processable images found on page {PageNumber}", page.Number);
            return descriptions;
        }

        for (var i = 0; i < images.Length - 1; i++)
        {
            _logger.LogInformation("Processing image {ImageIndex} on page {PageNumber}", i, page.Number);
            var image = images[i + 1];
            var imageBytes = GetImageBytes(image);
            var description = await GetImageDescription(imageBytes);
            descriptions.Add((i, description));
            _logger.LogInformation("Image {ImageIndex} processed. Description length: {Length}",
                i, description.Length);
        }

        _logger.LogInformation("Completed processing {Count} images on page {PageNumber}",
            descriptions.Count, page.Number);
        return descriptions;
    }

    private static byte[] GetImageBytes(UglyToad.PdfPig.Content.IPdfImage image)
    {
        return image.TryGetPng(out var bytes) ? bytes! : image.RawBytes.ToArray();
    }

    private async Task<string> GetImageDescription(byte[] imageBytes)
    {
        _logger.LogInformation("Starting image description generation. Image size: {Size} bytes", imageBytes.Length);

        var description = await _semanticKernelClient.ExecuteVisionPrompt(
            ModelConfiguration.Gpt4o_Mini_202407,
            null,
            IMAGE_ANALYSIS_PROMPT,
            [new ReadOnlyMemory<byte>(imageBytes)],
            1000,
            temperature: 0.2,
            cancellationToken: default);

        if (string.IsNullOrEmpty(description))
        {
            _logger.LogWarning("Failed to generate image description");
            throw new Exception("Failed to describe image");
        }

        _logger.LogInformation("Image description generated successfully. Length: {Length} chars",
            description.Length);
        return description;
    }

    private static string CleanupText(string text)
    {
        return Regex.Replace(text.Trim(), @"\s+", " ");
    }

    private async Task<string> CreateDocumentSummary(IReadOnlyCollection<PageContent> pages, CancellationToken cancellationToken)
    {
        var fullContent = new StringBuilder();
        foreach (var page in pages)
        {
            fullContent.AppendLine(page.TextContent);
            foreach (var (_, description) in page.ImageDescriptions)
            {
                fullContent.AppendLine($"Image Description: {description}");
            }
        }

        return await _semanticKernelClient.ExecutePrompt(
            ModelConfiguration.Gpt4o_Mini_202407,
            null,
            $"{DOCUMENT_SUMMARY_PROMPT}\n{fullContent}",
            maxTokens: 2000,
            temperature: 0.3,
            cancellationToken: cancellationToken);
    }

    private static string BuildAnswerPrompt(string context, string question)
    {
        return $$"""
                 Jesteś pomocnym asystentem, który odpowiada na pytania w języku polskim, bazując wyłącznie na dostarczonym kontekście. Musisz:  

                 1. **Używać informacji z dostarczonego kontekstu oraz wiedzy ogólnej, łącząc je w logiczną całość, nawet jeśli informacja nie jest podana bezpośrednio.**  
                 2. Jeśli odpowiedź można znaleźć, wnioskowując na podstawie danych liczbowych, czasowych lub logicznych relacji w kontekście (np. daty, różnice czasowe, wydarzenia), precyzyjnie wykonaj takie wnioskowanie.    
                 3. Jeśli mimo to brak dostatecznych danych, aby odpowiedzieć, napisz „Nie mogę znaleźć odpowiedzi w dostępnym kontekście”.  
                 4. Odpowiadaj zwięźle i na temat.  
                 5. Odpowiadaj wyłącznie po polsku.  
                 6. Nie wspominaj o tym, że korzystasz z kontekstu, oraz nie dodawaj zastrzeżeń ani wyjaśnień.  
                 7. **W przypadku dat i liczb: dokładnie przeanalizuj podane okresy czasu, różnice i relacje między nimi, aby wywnioskować wynik. Data może nie być podana dosłownie, ale być zapisana w formie liczbowej lub tekstowej np. `2 dni temu` lub `za rok`.**
                   7.1 Może się zdarzyć, że odpowiedzią będzie zakres dat, np. po 2020 roku, albo w ciągu ostatnich 5 lat.
                 8. ZAWSZE Myśl głośno, krok po kroku, aby wyjaśnić swoje wnioski. Najpierw wypisz fakty które mogą pomóc w odpowiedzi, następnie krok po kroku je przeanalizuj. Bądź ekstra uważny szczególnie przy odpowiadaniu na pytania o daty - liczy się jak największa dokładność, szczególnie przy określaniu zakresów dat. Proces myślenia umieść w tagach `<MYSLE_GLOSNO></MYSLE_GLOSNO>`. 
                   8.1 . Jeśli okaże się w kontekście nie ma bezpośredniej odpowiedzi na pytanie użyj dedukcji i umiejętności logicznego myślenia, aby znaleźć odpowiedź. Jeśli nie jesteś do końca pewien, wybierz najbardziej prawdopodobną odpowiedź zaznaczając ją jako najbardziej prawdopodobną jednak nie pewną. 

                 ### **Podpowiedź:**  
                 Jeśli kontekst zawiera szczegółowe dane czasowe (np. rok, miesiąc, okres w latach), rozważ je matematycznie, aby znaleźć wynik, nawet jeśli pełna informacja nie jest podana dosłownie.

                 <FORMAT ODPOWIEDZI>
                 ZAWSZE Myśl głośno, krok po kroku, aby wyjaśnić swoje wnioski.
                 Najpierw wypisz fakty które mogą pomóc w odpowiedzi, następnie krok po kroku je przeanalizuj. 
            
                 Na końcu, zapisz swoją odpowiedź w formie krótkiego, zwięzłego zdania w tagach `<odpowiedz></odpowiedz>`.
                 
                 Jeśli okaże się w kontekście nie ma bezpośredniej odpowiedzi na pytanie i nie znalazłeś odpowiedzi spróbuj jeszcze raz powtarzając proces myślenia głośno użyj dedukcji i umiejętności logicznego myślenia oraz łączenia faktów, aby znaleźć odpowiedź. Jeśli nie jesteś do końca pewien, wybierz najbardziej prawdopodobną odpowiedź zaznaczając ją jako najbardziej.
                 Jeśli po drugiej próbie nie znajdziesz odpowiedzi, napisz „Nie mogę znaleźć odpowiedzi w dostępnym kontekście”.
                 </FORMAT ODPOWIEDZI>

                 ### Kontekst:
                 {{context}}

                 ### Pytanie:
                 {{question}}
                 """;
    }

    private const string IMAGE_ANALYSIS_PROMPT = """
                                                 Opisz wyłącznie widoczne elementy obrazu, skupiając się na:
                                                 1. Tekst - dokładna treść wszystkich napisów w języku polskim. Jeśli nie ma tekstu, zacznij opis od <BRAK TEKSTU>

                                                 Format opisu:
                                                 - Jeśli jest tekst, zacznij od jego dokładnego przepisania
                                                 - Nie interpretuj ani nie spekuluj
                                                 - Odpowiedź w języku polskim
                                                 """;

    private const string DOCUMENT_SUMMARY_PROMPT = """
                                                   Stwórz zwięzłe podsumowanie dokumentu wymieniając wyłącznie istniejące informacje w następujących kategoriach:
                                                   1. Osoby oraz ich działania i powiązania
                                                   2. Miejsca i lokalizacje
                                                   3. Daty i okresy czasowe
                                                   4. Wydarzenia i ich przebieg
                                                   5. Obiekty, technologie i ich zastosowania

                                                   Format odpowiedzi:
                                                   - Wypisz informacje w formie krótkich punktów
                                                   - Każda informacja musi zawierać konkretne fakty (kto, co, gdzie, kiedy)
                                                   - Pomijaj kategorie, dla których nie ma informacji
                                                   - Nie używaj zdań przeczących ani informacji o brakach
                                                   - Nie interpretuj ani nie spekuluj

                                                   Treść dokumentu:
                                                   """;
}
