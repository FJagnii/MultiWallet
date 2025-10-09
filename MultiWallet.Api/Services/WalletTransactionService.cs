using MultiWallet.Api.Exceptions;
using MultiWallet.Api.Models;
using MultiWallet.Api.Repositories;

namespace MultiWallet.Api.Services;

public class WalletTransactionService : IWalletTransactionService
{
    private readonly IExchangeRatesRepository _exchangeRatesRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletLockProvider _lockProvider;
    private readonly TimeSpan _waitForLockTimeout = TimeSpan.FromSeconds(5);

    public WalletTransactionService(IExchangeRatesRepository exchangeRatesRepository, IWalletRepository walletRepository,
        IWalletLockProvider lockProvider)
    {
        _exchangeRatesRepository = exchangeRatesRepository;
        _walletRepository = walletRepository;
        _lockProvider = lockProvider;
    }
    
    public async Task<CurrencyData> AddFundsAsync(int walletId, string currencyCode, decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                throw new WalletNotFoundException(walletId);
            }

            var currencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(currencyCode);
            if (currencyData == null)
            {
                throw new CurrencyNotFoundException(currencyCode);
            }

            var currencyToAddFundsTo = GetOrCreateCurrencyInWallet(wallet, currencyData);
            currencyToAddFundsTo.Amount += amount;

            await _walletRepository.UpdateWalletAsync(wallet);
            return currencyToAddFundsTo;
        }
    }

    public async Task<CurrencyData> WithdrawFundsAsync(int walletId, string currencyCode, decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                throw new WalletNotFoundException(walletId);
            }

            var currencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(currencyCode);
            if (currencyData == null)
            {
                throw new CurrencyNotFoundException(currencyCode);
            }

            var currencyToWithdrawFundsFrom = wallet.Currencies.FirstOrDefault(c => c.Code == currencyCode);
            if (currencyToWithdrawFundsFrom == null)
            {
                throw new CurrencyNotInWalletException(currencyCode);
            }

            if (currencyToWithdrawFundsFrom.Amount < amount)
            {
                throw new NotEnoughFundsException(currencyToWithdrawFundsFrom.Amount);
            }

            currencyToWithdrawFundsFrom.Amount -= amount;

            await _walletRepository.UpdateWalletAsync(wallet);
            return currencyToWithdrawFundsFrom;
        }
    }

    public async Task<(CurrencyData finalizedSourceCurrency, CurrencyData finalizedTargetCurrency)> ExchangeFromFundsAsync(
        int walletId, string sourceCurrencyCode, string targetCurrencyCode, decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                throw new WalletNotFoundException(walletId);
            }

            var sourceCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(sourceCurrencyCode);
            if (sourceCurrencyData == null)
            {
                throw new CurrencyNotFoundException(sourceCurrencyCode);
            }

            var targetCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(targetCurrencyCode);
            if (targetCurrencyData == null)
            {
                throw new CurrencyNotFoundException(targetCurrencyCode);
            }

            var currencyToExchangeFundsFrom = wallet.Currencies.FirstOrDefault(c => c.Code == sourceCurrencyCode);
            if (currencyToExchangeFundsFrom == null)
            {
                throw new CurrencyNotInWalletException(sourceCurrencyCode);
            }

            if (currencyToExchangeFundsFrom.Amount < amount)
            {
                throw new NotEnoughFundsException(currencyToExchangeFundsFrom.Amount);
            }

            var currencyToExchangeFundsTo = GetOrCreateCurrencyInWallet(wallet, targetCurrencyData);
            //konwersja źródłowej waluty przez jej przelicznik na PLN
            decimal exchangeAmountInPln = amount * sourceCurrencyData.Mid;
            //konwersja z PLN na docelową walutę przez jej przelicznik
            decimal exchangeAmountInTargetCurrency = exchangeAmountInPln / targetCurrencyData.Mid;

            currencyToExchangeFundsFrom.Amount -= amount;
            currencyToExchangeFundsTo.Amount += exchangeAmountInTargetCurrency;

            await _walletRepository.UpdateWalletAsync(wallet);
            return (currencyToExchangeFundsFrom, currencyToExchangeFundsTo);
        }
    }

    public async Task<(CurrencyData finalizedSourceCurrency, CurrencyData finalizedTargetCurrency)> ExchangeToFundsAsync(
        int walletId, string sourceCurrencyCode, string targetCurrencyCode, decimal amount)
    {
        using (await _lockProvider.LockWalletAsync(walletId, _waitForLockTimeout))
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                throw new WalletNotFoundException(walletId);
            }

            var sourceCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(sourceCurrencyCode);
            if (sourceCurrencyData == null)
            {
                throw new CurrencyNotFoundException(sourceCurrencyCode);
            }

            var targetCurrencyData = await _exchangeRatesRepository.GetCurrencyDataAsync(targetCurrencyCode);
            if (targetCurrencyData == null)
            {
                throw new CurrencyNotFoundException(targetCurrencyCode);
            }

            var currencyToExchangeFundsFrom = wallet.Currencies.FirstOrDefault(c => c.Code == sourceCurrencyCode);
            if (currencyToExchangeFundsFrom == null)
            {
                throw new CurrencyNotInWalletException(sourceCurrencyCode);
            }

            //konwersja kwoty w docelowej walucie na PLN przez jej przelicznik
            decimal exchangeAmountInPln = amount * targetCurrencyData.Mid;
            //konwersja z PLN na kwotę w źródłowej walucie przez jej przelicznik
            decimal exchangeAmountInSourceCurrency = exchangeAmountInPln / sourceCurrencyData.Mid;

            if (currencyToExchangeFundsFrom.Amount < exchangeAmountInSourceCurrency)
            {
                throw new NotEnoughFundsException(currencyToExchangeFundsFrom.Amount);
            }

            var currencyToExchangeFundsTo = GetOrCreateCurrencyInWallet(wallet, targetCurrencyData);
            currencyToExchangeFundsFrom.Amount -= exchangeAmountInSourceCurrency;
            currencyToExchangeFundsTo.Amount += amount;

            await _walletRepository.UpdateWalletAsync(wallet);
            return (currencyToExchangeFundsFrom, currencyToExchangeFundsTo);
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