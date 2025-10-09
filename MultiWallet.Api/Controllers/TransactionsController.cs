using Microsoft.AspNetCore.Mvc;
using MultiWallet.Api.Models;
using MultiWallet.Api.Requests;
using MultiWallet.Api.Services;

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
        return Ok(fundsAdded);
    }

    [HttpPost("withdrawFunds")]
    public async Task<ActionResult<CurrencyData>> WithdrawFunds(int walletId, WithdrawFundsRequest withdrawFundsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var fundsWithdrawn = await _transactionService.WithdrawFundsAsync(walletId, withdrawFundsRequest.CurrencyCode, withdrawFundsRequest.Amount);
        return Ok(fundsWithdrawn);
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
        return Ok(new List<CurrencyData> {fundsExchanged.finalizedSourceCurrency, fundsExchanged.finalizedTargetCurrency});
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
        return Ok(new List<CurrencyData> {fundsExchanged.finalizedSourceCurrency, fundsExchanged.finalizedTargetCurrency});
    }
}