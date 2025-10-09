namespace MultiWallet.Api.Exceptions;

public class WalletNotFoundException : Exception
{
    public WalletNotFoundException(int walletId) : base($"Nie znaleziono portfela o id: {walletId}") { }
    
    public WalletNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}