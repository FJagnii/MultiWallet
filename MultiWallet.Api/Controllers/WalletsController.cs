using Microsoft.AspNetCore.Mvc;
using MultiWallet.Api.Models;
using MultiWallet.Api.Requests;
using MultiWallet.Api.Services;
using MultiWallet.Api.Services.WalletOperationResponses;

namespace MultiWallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost]
    public async Task<ActionResult<Wallet>> CreateWallet(CreateWalletRequest createWalletRequest)
    {
        var wallet = await _walletService.CreateWalletAsync(createWalletRequest.WalletName, createWalletRequest.Currencies);
        return Created($"/wallets/{wallet.Id}", wallet);
    }

    [HttpGet]
    public async Task<ActionResult<List<Wallet>>> GetWallets()
    {
        var wallets = await _walletService.GetWalletsAsync();
        return Ok(wallets);
    }

    [HttpGet("{walletId}")]
    public async Task<ActionResult<Wallet>> GetWallet(int walletId)
    {
        var wallet = await _walletService.GetWalletAsync(walletId);
        if (wallet == null)
        {
            return NotFound($"Nie znaleziono portfela o id {walletId}");
        }

        return Ok(wallet);
    }

    [HttpPost("{walletId}/addFunds")]
    public async Task<ActionResult<CurrencyData>> AddFunds(int walletId, AddFundsRequest walletOperationRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var fundsAdded = await _walletService.AddFundsAsync(walletId, walletOperationRequest.CurrencyCode, walletOperationRequest.Amount);
        
        if (fundsAdded.ResponseCode == WalletOperationResponseCode.WalletNotFound)
        {
            return NotFound($"Nie znaleziono portfela o id: {walletId}");
        }

        if (fundsAdded.ResponseCode == WalletOperationResponseCode.CurrencyDoesNotExist)
        {
            return BadRequest($"Nie istnieje waluta: {walletOperationRequest.CurrencyCode}");
        }

        return Ok(fundsAdded.CurrentBalance);
    }

    [HttpPost("{walletId}/withdrawFunds")]
    public async Task<ActionResult<CurrencyData>> WithdrawFunds(int walletId, WithdrawFundsRequest withdrawFundsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var fundsWithdrawn = await _walletService.WithdrawFundsAsync(walletId, withdrawFundsRequest.CurrencyCode, withdrawFundsRequest.Amount);
        
        if (fundsWithdrawn.ResponseCode == WalletOperationResponseCode.WalletNotFound)
        {
            return NotFound($"Nie znaleziono portfela o id: {walletId}");
        }

        if (fundsWithdrawn.ResponseCode == WalletOperationResponseCode.CurrencyDoesNotExist)
        {
            return BadRequest($"Nie istnieje waluta: {withdrawFundsRequest.CurrencyCode}");
        }

        if (fundsWithdrawn.ResponseCode == WalletOperationResponseCode.CurrencyNotInWallet)
        {
            return BadRequest($"Portfel nie zawiera środków w walucie: {withdrawFundsRequest.CurrencyCode}");
        }

        if (fundsWithdrawn.ResponseCode == WalletOperationResponseCode.NotEnoughFunds)
        {
            return BadRequest($"Brak wystarczających środków ({fundsWithdrawn.CurrentBalance.Amount})");
        }

        return Ok(fundsWithdrawn.CurrentBalance);
    }

    [HttpPost("{walletId}/exchangeFromFunds")]
    public async Task<ActionResult<List<CurrencyData>>> ExchangeFromFunds(int walletId, ExchangeFundsRequest exchangeFundsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (exchangeFundsRequest.SourceCurrencyCode == exchangeFundsRequest.TargetCurrencyCode)
        {
            return BadRequest("Nie można wymienić waluty na tę samą walutę");
        }

        var fundsExchanged = await _walletService.ExchangeFromFundsAsync(walletId, exchangeFundsRequest.SourceCurrencyCode,
            exchangeFundsRequest.TargetCurrencyCode, exchangeFundsRequest.Amount);

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.WalletNotFound)
        {
            return NotFound($"Nie znaleziono portfela o id: {walletId}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.SourceCurrencyDoesNotExist)
        {
            return BadRequest($"Nie istnieje waluta: {exchangeFundsRequest.SourceCurrencyCode}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.TargetCurrencyDoesNotExist)
        {
            return BadRequest($"Nie istnieje waluta: {exchangeFundsRequest.TargetCurrencyCode}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.SourceCurrencyNotInWallet)
        {
            return BadRequest($"Portfel nie zawiera środków w walucie: {exchangeFundsRequest.SourceCurrencyCode}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.NotEnoughFunds)
        {
            return BadRequest($"Brak wystarczających środków do wykonania wymiany ({fundsExchanged.CurrentSourceCurrencyBalance.Amount})");
        }
       
        return Ok(new List<CurrencyData> {fundsExchanged.CurrentSourceCurrencyBalance, fundsExchanged.CurrentTargetCurrencyBalance});
    }

    [HttpPost("{walletId}/exchangeToFunds")]
    public async Task<ActionResult<List<CurrencyData>>> ExchangeToFunds(int walletId, ExchangeFundsRequest exchangeFundsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (exchangeFundsRequest.SourceCurrencyCode == exchangeFundsRequest.TargetCurrencyCode)
        {
            return BadRequest("Nie można wymienić waluty na tę samą walutę");
        }

        var fundsExchanged = await _walletService.ExchangeToFundsAsync(walletId, exchangeFundsRequest.SourceCurrencyCode,
            exchangeFundsRequest.TargetCurrencyCode, exchangeFundsRequest.Amount);

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.WalletNotFound)
        {
            return NotFound($"Nie znaleziono portfela o id: {walletId}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.SourceCurrencyDoesNotExist)
        {
            return BadRequest($"Nie istnieje waluta: {exchangeFundsRequest.SourceCurrencyCode}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.TargetCurrencyDoesNotExist)
        {
            return BadRequest($"Nie istnieje waluta: {exchangeFundsRequest.TargetCurrencyCode}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.SourceCurrencyNotInWallet)
        {
            return BadRequest($"Portfel nie zawiera środków w walucie: {exchangeFundsRequest.SourceCurrencyCode}");
        }

        if (fundsExchanged.ResponseCode == ExchangeFundsResponseCode.NotEnoughFunds)
        {
            return BadRequest($"Brak wystarczających środków źródłowych do wykonania wymiany ({fundsExchanged.CurrentSourceCurrencyBalance.Amount})");
        }
       
        return Ok(new List<CurrencyData> {fundsExchanged.CurrentSourceCurrencyBalance, fundsExchanged.CurrentTargetCurrencyBalance});
    }
}