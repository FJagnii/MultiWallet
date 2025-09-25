using System.Text.Json;
using Microsoft.Extensions.Options;
using MultiWallet.Api.Configuration;
using MultiWallet.Api.Exceptions;
using MultiWallet.Api.Models;

namespace MultiWallet.Api.Repositories.FileRepositories;

public class FileExchangeRatesRepository : IExchangeRatesRepository
{
    private List<NbpCurrencyData> _currentExchangeRates;
    private readonly string _exchangeRatesFilePath;
    private static readonly object _lock = new object();
    private readonly ILogger<FileExchangeRatesRepository> _logger;

    public FileExchangeRatesRepository(IOptions<ExchangeRatesFileConfig> exchangeRatesFileOptions, ILogger<FileExchangeRatesRepository> logger)
    {
        _exchangeRatesFilePath = exchangeRatesFileOptions.Value.FilePath;
        _logger = logger;

        lock (_lock)
        {
            //jeśli plik istnieje i udało się go odczytać to będziemy mieć w pamięci dane walut NBP
            //jeśli plik nie istnieje lub złapano z jakiegoś powodu wyjątek to zmienna po starcie API pozostanie nullem
            //jeśli później nastąpi próba odczytu walut (z nullowej zmiennej) to jest to odpowiednio zabezpieczone w metodzie GetCurrencyDataAsync() 
            try
            {
                if (File.Exists(_exchangeRatesFilePath))
                {
                    string json = File.ReadAllText(_exchangeRatesFilePath);
                    _currentExchangeRates = JsonSerializer.Deserialize<List<NbpCurrencyData>>(json);
                }
                else
                {
                    using var file = File.Create(_exchangeRatesFilePath);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Złapano wyjątek przy próbie odczytu pliku walut NBP z pliku");
            }
        }
    }

    public Task SaveDataAsync(List<NbpCurrencyData> exchangeRates)
    {
        lock (_lock)
        {
            try
            {
                _currentExchangeRates = exchangeRates;
                //jeśli uda się przypisać nowe wartości walut, ale nie uda się ich zapisać do pliku
                //to w pamięci mamy świeżo pobrane dane, a jeśli w przyszłości po restarcie API odczytane zostaną dane nieaktualne
                //bądź dane w ogóle nie zostaną odczytane to codziennie są one na nowo pobierane z API NBP, więc nastąpi synchronizacja
                string json = JsonSerializer.Serialize(exchangeRates);
                File.WriteAllText(_exchangeRatesFilePath, json);
            }
            catch (Exception e)
            {
                throw new ExchangeRatesRepositoryException("Nieoczekiwany błąd przy zapisie pliku walut NBP", e);
            }
            
            return Task.CompletedTask;
        }
    }

    public Task<NbpCurrencyData> GetCurrencyDataAsync(string currencyCode)
    {
        lock (_lock)
        {
            try
            {
                return Task.FromResult(_currentExchangeRates.FirstOrDefault(c => c.Code == currencyCode));
            }
            catch (NullReferenceException e)
            {
                throw new ExchangeRatesRepositoryException("Brak danych walut NBP", e);
            }
        }
    }
}