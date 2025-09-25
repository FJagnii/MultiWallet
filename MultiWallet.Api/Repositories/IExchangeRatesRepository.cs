using MultiWallet.Api.Models;

namespace MultiWallet.Api.Repositories;

public interface IExchangeRatesRepository
{
    Task SaveDataAsync(List<NbpCurrencyData> exchangeRates);
    Task<NbpCurrencyData> GetCurrencyDataAsync(string currencyCode);
}