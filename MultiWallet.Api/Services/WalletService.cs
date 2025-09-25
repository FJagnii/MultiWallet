using MultiWallet.Api.Models;
using MultiWallet.Api.Repositories;
using MultiWallet.Api.Services.WalletOperationResponses;

namespace MultiWallet.Api.Services;

public class WalletService : IWalletService
{
    private readonly IExchangeRatesRepository _exchangeRatesRepository;
    private readonly ILogger<WalletService> _logger;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletLockProvider _lockProvider;
    private readonly TimeSpan _waitForLockTimeout = TimeSpan.FromSeconds(5);

    public WalletService(IExchangeRatesRepository exchangeRatesRepository, ILogger<WalletService> logger,
        IWalletRepository walletRepository, IWalletLockProvider lockProvider)
    {
        _exchangeRatesRepository = exchangeRatesRepository;
        _logger = logger;
        _walletRepository = walletRepository;
        _lockProvider = lockProvider;
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

    public async Task<AddFundsResponse> AddFundsAsync(int walletId, string currencyCode, decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                return AddFundsResponse.WalletNotFound();
            }

            var currencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(currencyCode);
            if (currencyData == null)
            {
                return AddFundsResponse.CurrencyDoesNotExist();
            }

            var currencyToAddFundsTo = GetOrCreateCurrencyInWallet(wallet, currencyData);
            currencyToAddFundsTo.Amount += amount;

            await _walletRepository.UpdateWalletAsync(wallet);
            return AddFundsResponse.Success(currencyToAddFundsTo);
        }
    }

    public async Task<WithdrawFundsResponse> WithdrawFundsAsync(int walletId, string currencyCode, decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                return WithdrawFundsResponse.WalletNotFound();
            }

            var currencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(currencyCode);
            if (currencyData == null)
            {
                return WithdrawFundsResponse.CurrencyDoesNotExist();
            }

            var currencyToWithdrawFundsFrom = wallet.Currencies.FirstOrDefault(c => c.Code == currencyCode);
            if (currencyToWithdrawFundsFrom == null)
            {
                return WithdrawFundsResponse.CurrencyNotInWallet();
            }

            if (currencyToWithdrawFundsFrom.Amount < amount)
            {
                return WithdrawFundsResponse.NotEnoughFunds(currencyToWithdrawFundsFrom);
            }

            currencyToWithdrawFundsFrom.Amount -= amount;

            await _walletRepository.UpdateWalletAsync(wallet);
            return WithdrawFundsResponse.Success(currencyToWithdrawFundsFrom);
        }
    }

    public async Task<ExchangeFundsResponse> ExchangeFromFundsAsync(int walletId, string sourceCurrencyCode, string targetCurrencyCode,
        decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                return ExchangeFundsResponse.WalletNotFound();
            }

            var sourceCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(sourceCurrencyCode);
            if (sourceCurrencyData == null)
            {
                return ExchangeFundsResponse.SourceCurrencyDoesNotExist();
            }

            var targetCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(targetCurrencyCode);
            if (targetCurrencyData == null)
            {
                return ExchangeFundsResponse.TargetCurrencyDoesNotExist();
            }

            var currencyToExchangeFundsFrom = wallet.Currencies.FirstOrDefault(c => c.Code == sourceCurrencyCode);
            if (currencyToExchangeFundsFrom == null)
            {
                return ExchangeFundsResponse.SourceCurrencyNotInWallet();
            }

            if (currencyToExchangeFundsFrom.Amount < amount)
            {
                return ExchangeFundsResponse.NotEnoughFunds(currencyToExchangeFundsFrom);
            }

            var currencyToExchangeFundsTo = GetOrCreateCurrencyInWallet(wallet, targetCurrencyData);
            //konwersja źródłowej waluty przez jej przelicznik na PLN
            decimal exchangeAmountInPln = amount * sourceCurrencyData.Mid;
            //konwersja z PLN na docelową walutę przez jej przelicznik
            decimal exchangeAmountInTargetCurrency = exchangeAmountInPln / targetCurrencyData.Mid;

            currencyToExchangeFundsFrom.Amount -= amount;
            currencyToExchangeFundsTo.Amount += exchangeAmountInTargetCurrency;

            await _walletRepository.UpdateWalletAsync(wallet);
            return ExchangeFundsResponse.Success(currencyToExchangeFundsFrom, currencyToExchangeFundsTo);
        }
    }

    public async Task<ExchangeFundsResponse> ExchangeToFundsAsync(int walletId, string sourceCurrencyCode, string targetCurrencyCode,
        decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                return ExchangeFundsResponse.WalletNotFound();
            }

            var sourceCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(sourceCurrencyCode);
            if (sourceCurrencyData == null)
            {
                return ExchangeFundsResponse.SourceCurrencyDoesNotExist();
            }

            var targetCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(targetCurrencyCode);
            if (targetCurrencyData == null)
            {
                return ExchangeFundsResponse.TargetCurrencyDoesNotExist();
            }

            var currencyToExchangeFundsFrom = wallet.Currencies.FirstOrDefault(c => c.Code == sourceCurrencyCode);
            if (currencyToExchangeFundsFrom == null)
            {
                return ExchangeFundsResponse.SourceCurrencyNotInWallet();
            }

            //konwersja kwoty w docelowej walucie na PLN przez jej przelicznik
            decimal exchangeAmountInPln = amount * targetCurrencyData.Mid;
            //konwersja z PLN na kwotę w źródłowej walucie przez jej przelicznik
            decimal exchangeAmountInSourceCurrency = exchangeAmountInPln / sourceCurrencyData.Mid;

            if (currencyToExchangeFundsFrom.Amount < exchangeAmountInSourceCurrency)
            {
                return ExchangeFundsResponse.NotEnoughFunds(currencyToExchangeFundsFrom);
            }

            var currencyToExchangeFundsTo = GetOrCreateCurrencyInWallet(wallet, targetCurrencyData);
            currencyToExchangeFundsFrom.Amount -= exchangeAmountInSourceCurrency;
            currencyToExchangeFundsTo.Amount += amount;

            await _walletRepository.UpdateWalletAsync(wallet);
            return ExchangeFundsResponse.Success(currencyToExchangeFundsFrom, currencyToExchangeFundsTo);
        }
    }

    private CurrencyData GetOrCreateCurrencyInWallet(Wallet wallet, NbpCurrencyData nbpCurrencyData)
    {
        var currencyInWallet = wallet.Currencies.FirstOrDefault(c => c.Code == nbpCurrencyData.Code);
        if (currencyInWallet == null)
        {
            CurrencyData newCurrencyData = new CurrencyData()
            {
                Name = nbpCurrencyData.Currency,
                Code = nbpCurrencyData.Code
            };
            currencyInWallet = newCurrencyData;
            wallet.Currencies.Add(newCurrencyData);
        }

        return currencyInWallet;
    }
}