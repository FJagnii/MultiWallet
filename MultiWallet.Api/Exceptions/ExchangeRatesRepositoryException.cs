namespace MultiWallet.Api.Exceptions;

public class ExchangeRatesRepositoryException : Exception
{
    public ExchangeRatesRepositoryException() { }
    public ExchangeRatesRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}