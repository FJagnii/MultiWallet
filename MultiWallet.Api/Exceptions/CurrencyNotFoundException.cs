namespace MultiWallet.Api.Exceptions;

public class CurrencyNotFoundException : Exception
{
    public CurrencyNotFoundException(string currencyCode) : base($"Nie znaleziono waluty: {currencyCode}") { }
    
    public CurrencyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}