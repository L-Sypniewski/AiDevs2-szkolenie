namespace AiDevs3.Tasks.S03E01___Dokumenty;

public class S03E01 : Lesson
{
    public S03E01(
        IConfiguration configuration,
        HttpClient httpClient) : base(configuration, httpClient) { }

    protected override string LessonName => "S03E01 — Dokumenty";


    protected override Delegate GetAnswerDelegate => async () =>
    {
        var responses = new Dictionary<string, string>();

        responses["2024-11-12_report-00-sektor_C4.txt"] =
            "sektor C4, Aleksander Ragowski, nauczyciel, język angielski, Szkoła Podstawowa nr 9, Grudziądz, krytyk automatyzacji, programista, język Java, działacz ruchu oporu, przeciwnik sztucznej inteligencji";
        responses["2024-11-12_report-01-sektor_A1.txt"] =
            "godzina, alarm, wykrycie, ruch organiczny, analiza wizualna, analiza sensoryczna, zwierzyna leśna, fałszywy alarm, obszar, patrol";
        responses["2024-11-12_report-02-sektor_A3.txt"] =
            "sektor A3, godzina, patrol, noc, monitoring, peryferie, obiekt, aktywność organiczna, aktywność mechaniczna, cisza, zadania";
        responses["2024-11-12_report-03-sektor_A3.txt"] =
            "Sektor A3, godzina, patrol, noc, poziom monitorowany, czujnik, życie organiczne, wykrywanie, rezultat, zakłócenie, stan";
        responses["2024-11-12_report-04-sektor_B2.txt"] =
            "sektor B2, godzina, patrol, zachodnia część, teren, anomalia, odchylenie, norma, sektor, bezpieczeństwo, kanały komunikacyjne";
        responses["2024-11-12_report-05-sektor_C1.txt"] =
            "sektor C1, godzina, aktywność organiczna, aktywność technologiczna, sensor dźwiękowy, detektor ruchu, gotowość, sygnały, patrol, monitorowanie, kontynuacja";
        responses["2024-11-12_report-06-sektor_C2.txt"] =
            "sektor C2, godzina, sektor północno-zachodni, stan obszaru, stabilność, skaner temperatury, skaner ruchu, brak wykrycia, jednostka operacyjna, patrol, powrót";
        responses["2024-11-12_report-07-sektor_C4.txt"] =
            "Barbara Zawadzka, specjalistka frontend developementu, ruch oporu, JavaScript, Python, sztuczna inteligencja, bazy wektorowe, sabotowanie systemów, zabezpieczenia systemów rządowych, nadajnik ultradźwiękowy";
        responses["2024-11-12_report-08-sektor_A1.txt"] =
            "sektor A1, monitoring, obszar patrolowy, cisza, czujnik, aktywność, obserwacja, teren, wytyczne, godzina, patrol";
        responses["2024-11-12_report-09-sektor_C2.txt"] = "patrol, peryferie zachodnie, czujnik, sygnał, anomalia, cykl, sektor";
        var response = await SubmitResults(taskName: "dokumenty", answer: responses);
        return TypedResults.Ok(response);
    };
}
