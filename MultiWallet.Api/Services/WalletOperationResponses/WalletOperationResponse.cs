using MultiWallet.Api.Models;

namespace MultiWallet.Api.Services.WalletOperationResponses;

public class WalletOperationResponse
{
    public WalletOperationResponseCode ResponseCode { get; set; }
    public CurrencyData CurrentBalance { get; set; }
}

public enum WalletOperationResponseCode
{
    Success,
    WalletNotFound,
    CurrencyDoesNotExist,

    CurrencyNotInWallet,
    NotEnoughFunds
}