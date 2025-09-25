using System.Collections.Concurrent;
using MultiWallet.Api.Exceptions;

namespace MultiWallet.Api.Services;

public class WalletLockProvider : IWalletLockProvider
{
    //semafory dla portfeli są przechowywane w słowniku na stałe, również takie, których walletId jest nieprawidłowy/nie istnieje
    //koszt pamięci jest niewielki, a mam gwarancję, że dla danego walletId zawsze zostanie użyty ten sam lock
    //w przypadku systemu z bardzo dużą liczbą portfeli potrzebny będzie mechanizm usuwania nieużywanych semaforów
    //mechanizm ten również rozwiąże problem semaforów dla portfeli, które nie istnieją
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> LockWalletAsync(int walletId, TimeSpan waitForLockTime)
    {
        var semaphore = _locks.GetOrAdd(walletId, _ => new SemaphoreSlim(1, 1));

        if (await semaphore.WaitAsync(waitForLockTime) == false)
        {
            throw new WalletLockTimeoutException(walletId, waitForLockTime);
        }

        return new Releaser(semaphore);
    }

    private class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public Releaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}