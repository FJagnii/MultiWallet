namespace MultiWallet.Api.Exceptions;

public class WalletLockTimeoutException : Exception
{
    public WalletLockTimeoutException(int walletId, TimeSpan waitForLockTime)
        : base($"Przekroczono limit czasu ({waitForLockTime}) oczekiwania na dostępność portfela o ID {walletId}")
    {
    }
}