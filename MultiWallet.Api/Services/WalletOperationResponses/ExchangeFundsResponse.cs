using MultiWallet.Api.Models;

namespace MultiWallet.Api.Services.WalletOperationResponses;

public class ExchangeFundsResponse
{
    public ExchangeFundsResponseCode ResponseCode { get; set; }
    public CurrencyData CurrentSourceCurrencyBalance { get; set; }
    public CurrencyData CurrentTargetCurrencyBalance { get; set; }

    public static ExchangeFundsResponse Success(CurrencyData currentSourceBalance, CurrencyData currentTargetBalance)
    {
        return new ExchangeFundsResponse()
        {
            ResponseCode = ExchangeFundsResponseCode.Success,
            CurrentSourceCurrencyBalance = currentSourceBalance,
            CurrentTargetCurrencyBalance = currentTargetBalance
        };
    }

    public static ExchangeFundsResponse WalletNotFound()
    {
        return new ExchangeFundsResponse() { ResponseCode = ExchangeFundsResponseCode.WalletNotFound };
    }

    public static ExchangeFundsResponse SourceCurrencyDoesNotExist()
    {
        return new ExchangeFundsResponse() { ResponseCode = ExchangeFundsResponseCode.SourceCurrencyDoesNotExist };
    }

    public static ExchangeFundsResponse TargetCurrencyDoesNotExist()
    {
        return new ExchangeFundsResponse() { ResponseCode = ExchangeFundsResponseCode.TargetCurrencyDoesNotExist };
    }

    public static ExchangeFundsResponse SourceCurrencyNotInWallet()
    {
        return new ExchangeFundsResponse() { ResponseCode = ExchangeFundsResponseCode.SourceCurrencyNotInWallet };
    }

    public static ExchangeFundsResponse NotEnoughFunds(CurrencyData currentSourceBalance)
    {
        return new ExchangeFundsResponse()
        {
            ResponseCode = ExchangeFundsResponseCode.NotEnoughFunds,
            CurrentSourceCurrencyBalance = currentSourceBalance
        };
    }
}

public enum ExchangeFundsResponseCode
{
    Success,
    WalletNotFound,
    SourceCurrencyDoesNotExist,
    TargetCurrencyDoesNotExist,
    SourceCurrencyNotInWallet,
    NotEnoughFunds
}