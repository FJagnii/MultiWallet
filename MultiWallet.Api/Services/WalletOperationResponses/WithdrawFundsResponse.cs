using MultiWallet.Api.Models;

namespace MultiWallet.Api.Services.WalletOperationResponses;

public class WithdrawFundsResponse : WalletOperationResponse
{
    public static WithdrawFundsResponse Success(CurrencyData currentBalance)
    {
        return new WithdrawFundsResponse()
        {
            ResponseCode = WalletOperationResponseCode.Success,
            CurrentBalance = currentBalance
        };
    }

    public static WithdrawFundsResponse WalletNotFound()
    {
        return new WithdrawFundsResponse() { ResponseCode = WalletOperationResponseCode.WalletNotFound };
    }

    public static WithdrawFundsResponse CurrencyDoesNotExist()
    {
        return new WithdrawFundsResponse() { ResponseCode = WalletOperationResponseCode.CurrencyDoesNotExist };
    }

    public static WithdrawFundsResponse CurrencyNotInWallet()
    {
        return new WithdrawFundsResponse() { ResponseCode = WalletOperationResponseCode.CurrencyNotInWallet };
    }

    public static WithdrawFundsResponse NotEnoughFunds(CurrencyData currentBalance)
    {
        return new WithdrawFundsResponse()
        {
            ResponseCode = WalletOperationResponseCode.NotEnoughFunds,
            CurrentBalance = currentBalance
        };
    }
}