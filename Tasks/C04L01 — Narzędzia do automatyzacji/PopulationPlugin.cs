using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public class PopulationPlugin
{
    private readonly HttpClient _httpClient;

    public PopulationPlugin(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [KernelFunction, Description("Given a country name return its population using its iso code.")]
    public async Task<decimal> GetPopulation([Description("Two-letter ISO code of the country")] string countryIsoCode)
    {
        var response = await _httpClient.GetFromJsonAsync<JsonNode>($"https://restcountries.com/v3.1/alpha/{countryIsoCode}?fields=population");
        var population = response?["population"]?.GetValue<int>();
        if (population is null)
        {
            throw new Exception("Could not retrieve the 'population' value from the response.");
        }

        return population.Value;
    }
}
