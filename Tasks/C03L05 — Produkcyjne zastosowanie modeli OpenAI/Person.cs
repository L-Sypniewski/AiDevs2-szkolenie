namespace AiDevs2_szkolenie.Tasks;

using System.Text.Json.Serialization;

public record Person
{
    [JsonPropertyName("imie")]
    public required string FirstName { get; init; }

    [JsonPropertyName("nazwisko")]
    public required string LastName { get; init; }

    [JsonPropertyName("o_mnie")]
    public required string Info { get; init; }

    [JsonPropertyName("ulubiony_kolor")]
    public required string FavouriteColor { get; init; }

    [JsonPropertyName("ulubione_jedzenie")]
    public string? FavouriteFood { get; init; }

    [JsonPropertyName("miejsce_zamieszkania")]
    public string? PlaceOfResidence { get; init; }
}
