using MultiWallet.Api.Clients;
using MultiWallet.Api.DTOs;
using MultiWallet.Api.Models;
using MultiWallet.Api.Repositories;

namespace MultiWallet.Api.Services;

public class NbpExchangeRatesFetcher : BackgroundService
{
    private readonly ILogger<NbpExchangeRatesFetcher> _logger;
    private readonly INbpCommunicationClient _nbpCommunicationClient;
    private readonly IServiceProvider _serviceProvider;
    
    public NbpExchangeRatesFetcher(INbpCommunicationClient nbpCommunicationClient, ILogger<NbpExchangeRatesFetcher> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _nbpCommunicationClient = nbpCommunicationClient;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var exchangeRatesTableDto = await _nbpCommunicationClient.GetExchangeRatesTableAsync();
                var exchangeRates = MapToNbpCurrencyDataList(exchangeRatesTableDto);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IExchangeRatesRepository>();
                    await repo.SaveDataAsync(exchangeRates);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Złapano wyjątek podczas cyklicznego pobierania danych z NBP");
            }

            //NBP aktualizuje tabelę co środę, ale istnieją wyjątki, gdy np. środa jest dniem wolnym od pracy,
            //dlatego dla uproszczenia i zabezpieczenia pobranie danych z tabeli następuje co 1 dzień
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private List<NbpCurrencyData> MapToNbpCurrencyDataList(NbpExchangeRatesTableDto exchangeRatesTableDto)
    {
        return exchangeRatesTableDto.Rates.Select(r => new NbpCurrencyData()
        {
            Code = r.Code,
            Currency = r.Currency,
            Mid = r.Mid
        }).ToList();
    }
}