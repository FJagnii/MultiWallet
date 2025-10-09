namespace MultiWallet.Api.Exceptions;

public class CurrencyNotInWalletException : Exception
{
    public CurrencyNotInWalletException(string currencyCode) : base($"Nie ma w portfelu waluty: {currencyCode}") { }
    
    public CurrencyNotInWalletException(string message, Exception innerException) : base(message, innerException) { }
}