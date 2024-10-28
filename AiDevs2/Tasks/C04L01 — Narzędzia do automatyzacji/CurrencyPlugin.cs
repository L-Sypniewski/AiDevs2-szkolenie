using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public class CurrencyPlugin
{
    private readonly HttpClient _httpClient;

    public CurrencyPlugin(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [KernelFunction, Description("Given a country name return the exchange rate for a given iso code to Polish Zloty (PLN). Shouldn't be used of one of the currencies is not Polish Zloty (PLN).")]
    public async Task<decimal> GetExchangeRate([Description("ISO code of the country")] string countryIsoCode)
    {
        var response = await _httpClient.GetFromJsonAsync<JsonNode>($"http://api.nbp.pl/api/exchangerates/rates/a/{countryIsoCode}/?format=json");
        var rates = response?["rates"]?.AsArray();
        if (rates is not { Count: > 0 })
        {
            throw new Exception("Could not retrieve the 'mid' value from the response.");
        }

        var midValue = rates.Single()?["mid"]?.GetValue<decimal>();
        return midValue ?? throw new Exception("Could not retrieve the 'mid' value from the response.");
    }
}
