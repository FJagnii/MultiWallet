namespace MultiWallet.Api.Services;

public interface IWalletLockProvider
{
    Task<IDisposable> LockWalletAsync(int walletId, TimeSpan waitForLockTime);
}