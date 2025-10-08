using MultiWallet.Api.Models;
using MultiWallet.Api.Repositories;
using MultiWallet.Api.Services;
using Moq;
using Microsoft.Extensions.Logging;

namespace MultiWallet.Tests;

public class WalletServiceTests
{
    private readonly Mock<IExchangeRatesRepository> _exchangeRatesRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly WalletService _walletService;

    public WalletServiceTests()
    {
        _exchangeRatesRepositoryMock = new Mock<IExchangeRatesRepository>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        var loggerMock = new Mock<ILogger<WalletService>>();

        _walletService = new WalletService(
            _exchangeRatesRepositoryMock.Object,
            loggerMock.Object,
            _walletRepositoryMock.Object
        );
    }
    
    [Fact]
    public async Task CreateWalletAsync_CreateWalletWithoutCurrencies()
    {
        //A
        var result = await _walletService.CreateWalletAsync("TestWallet");

        //A
        Assert.NotNull(result);
        Assert.Equal("TestWallet", result.Name);
        Assert.Empty(result.Currencies);
        _walletRepositoryMock.Verify(r => r.CreateWalletAsync(It.Is<Wallet>(w => w.Name == "TestWallet")), Times.Once);
    }
    
    [Fact]
    public async Task CreateWalletAsync_CreateWalletWithValidCurrencies()
    {
        //A
        var currencyCodes = new List<string> { "USD", "EUR" };
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD"))
            .ReturnsAsync(new NbpCurrencyData() { Currency = "Dolar", Code = "USD", Mid = 4.0m });

        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR"))
            .ReturnsAsync(new NbpCurrencyData() { Currency = "Euro", Code = "EUR", Mid = 4.5m });

        //A
        var result = await _walletService.CreateWalletAsync("TestWallet", currencyCodes);

        //A
        Assert.Equal("TestWallet", result.Name);
        Assert.Equal(2, result.Currencies.Count);
        Assert.Contains(result.Currencies, c => c.Code == "USD" && c.Name == "Dolar");
        Assert.Contains(result.Currencies, c => c.Code == "EUR" && c.Name == "Euro");
        _walletRepositoryMock.Verify(r => r.CreateWalletAsync(It.IsAny<Wallet>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateWalletAsync_SkipDuplicateCurrencies()
    {
        //A
        var currencyCodes = new List<string> { "USD", "USD", "USD", "EUR" };
        
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("USD"))
            .ReturnsAsync(new NbpCurrencyData() { Currency = "Dolar", Code = "USD", Mid = 4.0m });

        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("EUR"))
            .ReturnsAsync(new NbpCurrencyData() { Currency = "Euro", Code = "EUR", Mid = 4.5m });

        //A
        var result = await _walletService.CreateWalletAsync("TestWallet", currencyCodes);

        //A
        Assert.Equal("TestWallet", result.Name);
        Assert.Equal(2, result.Currencies.Count);
        Assert.Contains(result.Currencies, c => c.Code == "USD" && c.Name == "Dolar");
        Assert.Contains(result.Currencies, c => c.Code == "EUR" && c.Name == "Euro");
        _walletRepositoryMock.Verify(r => r.CreateWalletAsync(It.IsAny<Wallet>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateWalletAsync_SkipInvalidCurrencyCodes()
    {
        //A
        var currencyCodes = new List<string> { "ABC" };
        _exchangeRatesRepositoryMock.Setup(r => r.GetCurrencyDataAsync("ABC"))
            .ReturnsAsync((NbpCurrencyData)null);

        //A
        var result = await _walletService.CreateWalletAsync("TestWallet", currencyCodes);

        //A
        Assert.NotNull(result);
        Assert.Equal("TestWallet", result.Name);
        Assert.Empty(result.Currencies);
        _walletRepositoryMock.Verify(r => r.CreateWalletAsync(It.IsAny<Wallet>()), Times.Once);
    }

    [Fact]
    public async Task GetWalletsAsync()
    {
        //A
        Wallet a = new Wallet()
        {
            Name = "TestWallet"
        };
        Wallet b = new Wallet()
        {
            Name = "TestWallet2",
            Currencies = new()
            {
                new CurrencyData()
                {
                    Code = "USD",
                    Name = "Dolar",
                    Amount = 25m
                }
            }
        };
        _walletRepositoryMock.Setup(r => r.GetWalletsAsync())
            .ReturnsAsync(new List<Wallet>() { a, b });
        
        
        //A
        var result = await _walletService.GetWalletsAsync();
        
        //A
        Assert.Equal(2, result.Count);
        Assert.Equal("TestWallet", result[0].Name);
        Assert.Equal("TestWallet2", result[1].Name);
        Assert.Single(result[1].Currencies);
        Assert.Equal("USD", result[1].Currencies[0].Code);
        Assert.Equal("Dolar", result[1].Currencies[0].Name);
        Assert.Equal(25m, result[1].Currencies[0].Amount);
    }
    
    [Fact]
    public async Task GetWalletAsync_WalletDoesNotExist()
    {
        //A
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync((Wallet)null);
        
        //A
        var result = await _walletService.GetWalletAsync(1);
        
        //A
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetWalletAsync_ReturnsWallet()
    {
        //A
        Wallet wallet = new Wallet()
        {
            Name = "TestWallet",
            Currencies = new()
            {
                new CurrencyData()
                {
                    Code = "USD",
                    Name = "Dolar",
                    Amount = 25m
                }
            }
        };
        
        _walletRepositoryMock.Setup(r => r.GetWalletAsync(1)).ReturnsAsync(wallet);
        
        //A
        var result = await _walletService.GetWalletAsync(1);
        
        //A
        Assert.Equal("TestWallet", result.Name);
        Assert.Single(result.Currencies);
        Assert.Equal("USD", result.Currencies[0].Code);
        Assert.Equal("Dolar", result.Currencies[0].Name);
        Assert.Equal(25m, result.Currencies[0].Amount);
    }
}


