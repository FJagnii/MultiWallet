using MultiWallet.Api.Models;

namespace MultiWallet.Api.Services.WalletOperationResponses;

public class AddFundsResponse : WalletOperationResponse
{
    public static AddFundsResponse Success(CurrencyData currentBalance)
    {
        return new AddFundsResponse()
        {
            ResponseCode = WalletOperationResponseCode.Success,
            CurrentBalance = currentBalance
        };
    }

    public static AddFundsResponse WalletNotFound()
    {
        return new AddFundsResponse() { ResponseCode = WalletOperationResponseCode.WalletNotFound };
    }

    public static AddFundsResponse CurrencyDoesNotExist()
    {
        return new AddFundsResponse() { ResponseCode = WalletOperationResponseCode.CurrencyDoesNotExist };
    }
}