namespace MultiWallet.Api.Exceptions;

public class WalletRepositoryException : Exception
{
    public WalletRepositoryException() { }
    public WalletRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}