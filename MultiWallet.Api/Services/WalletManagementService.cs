using MultiWallet.Api.Models;
using MultiWallet.Api.Repositories;

namespace MultiWallet.Api.Services;

public class WalletManagementService : IWalletManagementService
{
    private readonly IExchangeRatesRepository _exchangeRatesRepository;
    private readonly ILogger<WalletManagementService> _logger;
    private readonly IWalletRepository _walletRepository;
    
    public WalletManagementService(IExchangeRatesRepository exchangeRatesRepository, ILogger<WalletManagementService> logger,
        IWalletRepository walletRepository)
    {
        _exchangeRatesRepository = exchangeRatesRepository;
        _logger = logger;
        _walletRepository = walletRepository;
    }

    public async Task<Wallet> CreateWalletAsync(string walletName, List<string> currencyCodes = null)
    {
        Wallet newWallet = new Wallet();
        newWallet.Name = walletName;

        if (currencyCodes != null && currencyCodes.Count > 0)
        {
            foreach (var currencyCode in currencyCodes)
            {
                var currencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(currencyCode);
                if (currencyData == null)
                {
                    _logger.LogWarning($"Tworzenie portfela: podana waluta {currencyCode} nie istnieje, pomijam");
                    continue;
                }

                if (newWallet.Currencies.Any(c => c.Code == currencyCode))
                {
                    _logger.LogWarning($"Tworzenie portfela: waluta {currencyCode} już istnieje w portfelu, pomijam");
                    continue;
                }

                newWallet.Currencies.Add(new CurrencyData()
                {
                    Name = currencyData.Currency,
                    Code = currencyData.Code,
                    Amount = 0
                });
            }
        }

        await _walletRepository.CreateWalletAsync(newWallet);
        return newWallet;
    }

    public async Task<List<Wallet>> GetWalletsAsync()
    {
        return await _walletRepository.GetWalletsAsync();
    }

    public async Task<Wallet> GetWalletAsync(int id)
    {
        return await _walletRepository.GetWalletAsync(id);
    }
}