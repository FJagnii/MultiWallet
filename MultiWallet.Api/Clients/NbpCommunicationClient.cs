using System.Text.Json;
using MultiWallet.Api.DTOs;

namespace MultiWallet.Api.Clients;

public class NbpCommunicationClient : INbpCommunicationClient
{
    private readonly HttpClient _httpClient;
    private readonly string _exchangeRatesTableUrl;

    public NbpCommunicationClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _exchangeRatesTableUrl = configuration.GetValue<string>("NbpExchangeRatesTableUrl");
    }

    public async Task<NbpExchangeRatesTableDto> GetExchangeRatesTableAsync()
    {
        var response = await _httpClient.GetAsync(_exchangeRatesTableUrl);
        response.EnsureSuccessStatusCode();
        var stringContent = await response.Content.ReadAsStringAsync();
        var deserializedData = JsonSerializer.Deserialize<List<NbpExchangeRatesTableDto>>(stringContent,
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        if (deserializedData == null || !deserializedData.Any())
        {
            throw new InvalidDataException("Odebrane dane tabeli z NBP były puste");
        }

        return deserializedData.First();
    }
}