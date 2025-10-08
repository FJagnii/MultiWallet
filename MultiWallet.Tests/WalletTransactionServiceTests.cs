using Moq;
using MultiWallet.Api.Models;
using MultiWallet.Api.Repositories;
using MultiWallet.Api.Services;
using MultiWallet.Api.Services.WalletOperationResponses;

namespace MultiWallet.Tests;

public class WalletTransactionServiceTests
{
    private readonly Mock<IExchangeRatesRepository> _exchangeRatesRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly WalletTransactionService _transactionService;
    
    public WalletTransactionServiceTests()
    {
        _exchangeRatesRepositoryMock = new Mock<IExchangeRatesRepository>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        var lockerMock = new Mock<IWalletLockProvider>();

        _transactionService = new WalletTransactionService(
            _exchangeRatesRepositoryMock.Object,
            _walletRepositoryMock.Object,
            lockerMock.Object
        );
    }
    
    [Fact]
    public async Task AddFundsAsync_WalletDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync((Wallet)null);

        //A
        var result = await _transactionService.AddFundsAsync(1, "USD", 100m);

        //A
        Assert.Equal(WalletOperationResponseCode.WalletNotFound, result.ResponseCode);
    }

    [Fact]
    public async Task AddFundsAsync_CurrencyDoesNotExist()
    {
        //A
        var wallet = new Wallet { Id = 1 };
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("XXX")).ReturnsAsync((NbpCurrencyData)null);

        //A
        var result = await _transactionService.AddFundsAsync(1, "XXX", 100m);

        //A
        Assert.Equal(WalletOperationResponseCode.CurrencyDoesNotExist, result.ResponseCode);
    }

    [Fact]
    public async Task AddFundsAsync_AddsAmountToExistingCurrency()
    {
        //A
        var wallet = new Wallet
        {
            Id = 1,
            Currencies = new List<CurrencyData>
            {
                new CurrencyData { Code = "USD", Amount = 50m }
            }
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD"))
            .ReturnsAsync(new NbpCurrencyData() { Code = "USD", Currency = "Dolar", Mid = 4m });

        //A
        var result = await _transactionService.AddFundsAsync(1, "USD", 25m);

        //A
        Assert.Equal(WalletOperationResponseCode.Success, result.ResponseCode);
        Assert.Equal(75m, wallet.Currencies.First(c => c.Code == "USD").Amount);
        Assert.Equal("USD", result.CurrentBalance.Code);
        Assert.Equal(75m, result.CurrentBalance.Amount);
        _walletRepositoryMock.Verify(r => r.UpdateWalletAsync(wallet), Times.Once);
    }

    [Fact]
    public async Task AddFundsAsync_AddsNewCurrencyAndAmount()
    {
        //A
        var wallet = new Wallet
        {
            Id = 1,
            Currencies = new List<CurrencyData>()
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR"))
            .ReturnsAsync(new NbpCurrencyData() { Code = "EUR", Currency = "Euro", Mid = 4m });

        //A
        var result = await _transactionService.AddFundsAsync(1, "EUR", 10m);

        //A
        Assert.Equal(WalletOperationResponseCode.Success, result.ResponseCode);
        var currencyData = wallet.Currencies.FirstOrDefault(c => c.Code == "EUR");
        Assert.NotNull(currencyData);
        Assert.Equal(10m, currencyData.Amount);
        Assert.Equal("EUR", currencyData.Code);
        Assert.Equal(10m, result.CurrentBalance.Amount);
        Assert.Equal("EUR", result.CurrentBalance.Code);
        _walletRepositoryMock.Verify(r => r.UpdateWalletAsync(wallet), Times.Once);
    }

    [Fact]
    public async Task WithdrawFundsAsync_WalletDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync((Wallet)null);

        //A
        var result = await _transactionService.WithdrawFundsAsync(1, "USD", 100m);

        //A
        Assert.Equal(WalletOperationResponseCode.WalletNotFound, result.ResponseCode);
    }

    [Fact]
    public async Task WithdrawFundsAsync_CurrencyDoesNotExist()
    {
        //A
        var wallet = new Wallet { Id = 1 };
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("XXX")).ReturnsAsync((NbpCurrencyData)null);

        //A
        var result = await _transactionService.WithdrawFundsAsync(1, "XXX", 100m);

        //A
        Assert.Equal(WalletOperationResponseCode.CurrencyDoesNotExist, result.ResponseCode);
    }

    [Fact]
    public async Task WithdrawFundsAsync_CurrencyNotInWallet()
    {
        //A
        var wallet = new Wallet
        {
            Id = 1,
            Currencies = new List<CurrencyData>()
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD"))
            .ReturnsAsync(new NbpCurrencyData() { Code = "USD", Currency = "Dolar", Mid = 4m });

        //A
        var result = await _transactionService.WithdrawFundsAsync(1, "USD", 25m);

        //A
        Assert.Equal(WalletOperationResponseCode.CurrencyNotInWallet, result.ResponseCode);
    }
    
    [Fact]
    public async Task WithdrawFundsAsync_NotEnoughFunds()
    {
        //A
        var wallet = new Wallet
        {
            Id = 1,
            Currencies = new List<CurrencyData>()
            {
                new CurrencyData { Code = "USD", Amount = 50m }
            }
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD"))
            .ReturnsAsync(new NbpCurrencyData() { Code = "USD", Currency = "Dolar", Mid = 4m });

        //A
        var result = await _transactionService.WithdrawFundsAsync(1, "USD", 100m);

        //A
        Assert.Equal(WalletOperationResponseCode.NotEnoughFunds, result.ResponseCode);
    }
    
    [Fact]
    public async Task WithdrawFundsAsync_WithdrawsAmount()
    {
        //A
        var wallet = new Wallet
        {
            Id = 1,
            Currencies = new List<CurrencyData>()
            {
                new CurrencyData { Code = "USD", Amount = 50m }
            }
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD"))
            .ReturnsAsync(new NbpCurrencyData() { Code = "USD", Currency = "Dolar", Mid = 4m });

        //A
        var result = await _transactionService.WithdrawFundsAsync(1, "USD", 10m);

        //A
        Assert.Equal(WalletOperationResponseCode.Success, result.ResponseCode);
        var currencyData = wallet.Currencies.FirstOrDefault(c => c.Code == "USD");
        Assert.Equal(40m, currencyData.Amount);
        Assert.Equal("USD", currencyData.Code);
        Assert.Equal(40m, result.CurrentBalance.Amount);
        Assert.Equal("USD", result.CurrentBalance.Code);
        _walletRepositoryMock.Verify(r => r.UpdateWalletAsync(wallet), Times.Once);
    }
    
    [Fact]
    public async Task ExchangeFromFundsAsync_WalletDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync((Wallet)null);

        //A
        var result = await _transactionService.ExchangeFromFundsAsync(1, "USD", "EUR", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.WalletNotFound, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeFromFundsAsync_SourceCurrencyDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(new Wallet());
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync((NbpCurrencyData)null);

        //A
        var result = await _transactionService.ExchangeFromFundsAsync(1, "USD", "EUR", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.SourceCurrencyDoesNotExist, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeFromFundsAsync_TargetCurrencyDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(new Wallet());
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData() { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("XYZ")).ReturnsAsync((NbpCurrencyData)null);

        //A
        var result = await _transactionService.ExchangeFromFundsAsync(1, "USD", "XYZ", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.TargetCurrencyDoesNotExist, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeFromFundsAsync_SourceCurrencyNotInWallet()
    {
        //A
        var wallet = new Wallet { Id = 1, Currencies = new() };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeFromFundsAsync(1, "USD", "EUR", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.SourceCurrencyNotInWallet, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeFromFundsAsync_NotEnoughFunds()
    {
        //A
        var wallet = new Wallet
        {
            Id = 1,
            Currencies = new List<CurrencyData>
            {
                new CurrencyData { Code = "USD", Amount = 50m }
            }
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeFromFundsAsync(1, "USD", "EUR", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.NotEnoughFunds, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeFromFundsAsync_ExchangeBetweenExistingCurrencies()
    {
        //A
        var wallet = new Wallet { Id = 1, Currencies = new List<CurrencyData>
            {
                new CurrencyData { Code = "USD", Name = "Dolar", Amount = 100m },
                new CurrencyData { Code = "EUR", Name = "Euro", Amount = 50m }
            } 
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData() { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeFromFundsAsync(1, "USD", "EUR", 40m);

        //A
        //przy zadanych przelicznikach 40 USD = 32 EUR
        Assert.Equal(ExchangeFundsResponseCode.Success, result.ResponseCode);
        Assert.Equal(60m, wallet.Currencies.FirstOrDefault(c => c.Code == "USD").Amount); // 100 USD - 40 USD = 60 USD
        Assert.Equal(82m, wallet.Currencies.FirstOrDefault(c => c.Code == "EUR").Amount); // 50 EUR + 32 EUR = 82 EUR
        
        Assert.Equal("USD", result.CurrentSourceCurrencyBalance.Code);
        Assert.Equal(60m, result.CurrentSourceCurrencyBalance.Amount); 
        Assert.Equal("EUR", result.CurrentTargetCurrencyBalance.Code);
        Assert.Equal(82m, result.CurrentTargetCurrencyBalance.Amount); 
    }
    
    [Fact]
    public async Task ExchangeFromFundsAsync_ExchangeWithNewCurrency()
    {
        //A
        var wallet = new Wallet { Id = 1, Currencies = new List<CurrencyData>
            {
                new CurrencyData { Code = "USD", Name = "Dolar", Amount = 100m }
            } 
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData() { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeFromFundsAsync(1, "USD", "EUR", 40m);

        //A
        //przy zadanych przelicznikach 40 USD = 32 EUR
        Assert.Equal(ExchangeFundsResponseCode.Success, result.ResponseCode);
        Assert.Equal(2, wallet.Currencies.Count);
        Assert.Equal(60m, wallet.Currencies.FirstOrDefault(c => c.Code == "USD").Amount); //100 USD - 40 USD = 60 USD
        Assert.Equal(32m, wallet.Currencies.FirstOrDefault(c => c.Code == "EUR").Amount); //0 EUR (nowa waluta w portfelu) + 32 EUR = 32 EUR
        
        Assert.Equal("USD", result.CurrentSourceCurrencyBalance.Code);
        Assert.Equal(60m, result.CurrentSourceCurrencyBalance.Amount);
        Assert.Equal("EUR", result.CurrentTargetCurrencyBalance.Code);
        Assert.Equal(32m, result.CurrentTargetCurrencyBalance.Amount);
    }
    
    [Fact]
    public async Task ExchangeToFundsAsync_WalletDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync((Wallet)null);

        //A
        var result = await _transactionService.ExchangeToFundsAsync(1, "USD", "EUR", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.WalletNotFound, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeToFundsAsync_SourceCurrencyDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(new Wallet());
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync((NbpCurrencyData)null);

        //A
        var result = await _transactionService.ExchangeToFundsAsync(1, "USD", "EUR", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.SourceCurrencyDoesNotExist, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeToFundsAsync_TargetCurrencyDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(new Wallet());
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData() { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("XYZ")).ReturnsAsync((NbpCurrencyData)null);

        //A
        var result = await _transactionService.ExchangeToFundsAsync(1, "USD", "XYZ", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.TargetCurrencyDoesNotExist, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeToFundsAsync_SourceCurrencyNotInWallet()
    {
        //A
        var wallet = new Wallet { Id = 1, Currencies = new() };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeToFundsAsync(1, "USD", "EUR", 100m);

        //A
        Assert.Equal(ExchangeFundsResponseCode.SourceCurrencyNotInWallet, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeToFundsAsync_NotEnoughSourceFunds()
    {
        //A
        var wallet = new Wallet
        {
            Id = 1,
            Currencies = new List<CurrencyData>
            {
                new CurrencyData { Code = "USD", Amount = 50m }
            }
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeToFundsAsync(1, "USD", "EUR", 50m);

        //A
        //przy zadanych przelicznikach 50 EUR = 62.5 USD
        Assert.Equal(ExchangeFundsResponseCode.NotEnoughFunds, result.ResponseCode);
    }

    [Fact]
    public async Task ExchangeToFundsAsync_ExchangeBetweenExistingCurrencies()
    {
        //A
        var wallet = new Wallet { Id = 1, Currencies = new List<CurrencyData>
            {
                new CurrencyData { Code = "USD", Name = "Dolar", Amount = 100m },
                new CurrencyData { Code = "EUR", Name = "Euro", Amount = 50m }
            } 
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData() { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeToFundsAsync(1, "USD", "EUR", 50m);

        //A
        //przy zadanych przelicznikach 50 EUR = 62.5 USD
        Assert.Equal(ExchangeFundsResponseCode.Success, result.ResponseCode);
        Assert.Equal(37.5m, wallet.Currencies.FirstOrDefault(c => c.Code == "USD").Amount); // 100 USD - 62.5 USD = 37.5 USD
        Assert.Equal(100m, wallet.Currencies.FirstOrDefault(c => c.Code == "EUR").Amount); // 50 EUR + 50 EUR = 100 EUR
        
        Assert.Equal("USD", result.CurrentSourceCurrencyBalance.Code);
        Assert.Equal(37.5m, result.CurrentSourceCurrencyBalance.Amount); 
        Assert.Equal("EUR", result.CurrentTargetCurrencyBalance.Code);
        Assert.Equal(100m, result.CurrentTargetCurrencyBalance.Amount); 
    }
    
    [Fact]
    public async Task ExchangeToFundsAsync_ExchangeWithNewCurrency()
    {
        //A
        var wallet = new Wallet { Id = 1, Currencies = new List<CurrencyData>
            {
                new CurrencyData { Code = "USD", Name = "Dolar", Amount = 100m }
            } 
        };

        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD")).ReturnsAsync(new NbpCurrencyData() { Code = "USD", Mid = 4m });
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR")).ReturnsAsync(new NbpCurrencyData { Code = "EUR", Mid = 5m });

        //A
        var result = await _transactionService.ExchangeToFundsAsync(1, "USD", "EUR", 50m);

        //A
        //przy zadanych przelicznikach 50 EUR = 62.5 USD
        Assert.Equal(ExchangeFundsResponseCode.Success, result.ResponseCode);
        Assert.Equal(2, wallet.Currencies.Count);
        Assert.Equal(37.5m, wallet.Currencies.FirstOrDefault(c => c.Code == "USD").Amount); //100 USD - 62.5 USD = 37.5 USD
        Assert.Equal(50m, wallet.Currencies.FirstOrDefault(c => c.Code == "EUR").Amount); //0 EUR (nowa waluta w portfelu) + 50 EUR = 50 EUR
        
        Assert.Equal("USD", result.CurrentSourceCurrencyBalance.Code);
        Assert.Equal(37.5m, result.CurrentSourceCurrencyBalance.Amount);
        Assert.Equal("EUR", result.CurrentTargetCurrencyBalance.Code);
        Assert.Equal(50m, result.CurrentTargetCurrencyBalance.Amount);
    }
}