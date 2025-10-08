using Microsoft.AspNetCore.Mvc;
using MultiWallet.Api.Models;
using MultiWallet.Api.Requests;
using MultiWallet.Api.Services;
using MultiWallet.Api.Services.WalletOperationResponses;

namespace MultiWallet.Api.Controllers;

[ApiController]
[Route("api/Wallets/{walletId}")]
public class TransactionsController : ControllerBase
{
    private readonly IWalletTransactionService _transactionService;
    
    public TransactionsController(IWalletTransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    
    [HttpPost("addFunds")]
    public async Task<ActionResult<CurrencyData>> AddFunds(int walletId, AddFundsRequest walletOperationRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var fundsAdded = await _transactionService.AddFundsAsync(walletId, walletOperationRequest.CurrencyCode, walletOperationRequest.Amount);
        
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

    [HttpPost("withdrawFunds")]
    public async Task<ActionResult<CurrencyData>> WithdrawFunds(int walletId, WithdrawFundsRequest withdrawFundsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var fundsWithdrawn = await _transactionService.WithdrawFundsAsync(walletId, withdrawFundsRequest.CurrencyCode, withdrawFundsRequest.Amount);
        
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

    [HttpPost("exchangeFromFunds")]
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

        var fundsExchanged = await _transactionService.ExchangeFromFundsAsync(walletId, exchangeFundsRequest.SourceCurrencyCode,
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

    [HttpPost("exchangeToFunds")]
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

        var fundsExchanged = await _transactionService.ExchangeToFundsAsync(walletId, exchangeFundsRequest.SourceCurrencyCode,
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