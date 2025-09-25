using MultiWallet.Api.Exceptions;
using MultiWallet.Api.Services;

namespace MultiWallet.Tests;

public class WalletLockProviderTests
{
    [Fact]
    public async Task LockWalletAsync_AllowAccess()
    {
        //A
        var provider = new WalletLockProvider();
        bool lockAcquired = false;
        
        //A
        using (await provider.LockWalletAsync(1, TimeSpan.FromSeconds(1)))
        {
            lockAcquired = true;
        }
        
        //A
        Assert.True(lockAcquired);
    }
    
    [Fact]
    public async Task LockWalletAsync_AccessBlocked()
    {
        //A
        var provider = new WalletLockProvider();
        bool task2AcquiredLock = false;
        WalletLockTimeoutException blockedTaskException = null;

        //A
        var task1 = Task.Run(async () =>
        {
            using (await provider.LockWalletAsync(1, TimeSpan.FromMilliseconds(100)))
            {
                //task1 blokuje portfel i utrzyma blokadę przez 500ms
                await Task.Delay(500);
            }
        });
        
        var task2 = Task.Run(async () =>
        {
            try
            {
                //task2 próbuje zablokować portfel i oczekuje blokady w ciągu 100ms (co się nie uda, bo portfel został zajęty przez task1 i będzie zajęty przez 500ms)
                using (await provider.LockWalletAsync(1, TimeSpan.FromMilliseconds(100)))
                {
                    //w przypadku sukcesu task2 podniesie flagę 
                    task2AcquiredLock = true;
                }
            }
            catch(WalletLockTimeoutException e)
            {
                blockedTaskException = e;
            }
        }); 
        
        await Task.WhenAll(task1, task2);
        
        //A
        Assert.False(task2AcquiredLock);
        Assert.NotNull(blockedTaskException);
    }
    
    [Fact]
    public async Task LockWalletAsync_AllowNextAccess()
    {
        //A
        var provider = new WalletLockProvider();
        bool task2AcquiredLock = false;
        WalletLockTimeoutException blockedTaskException = null;

        //A
        var task1 = Task.Run(async () =>
        {
            using (await provider.LockWalletAsync(1, TimeSpan.FromMilliseconds(500)))
            {
                //task1 blokuje portfel i utrzyma blokadę przez 100ms
                await Task.Delay(100);
            }
        });
        
        var task2 = Task.Run(async () =>
        {
            try
            {
                //task2 próbuje zablokować portfel i oczekuje blokady w ciągu 500ms (co ma się udać, gdyż task1 blokuje portfel tylko przez 100ms)
                using (await provider.LockWalletAsync(1, TimeSpan.FromMilliseconds(500)))
                {
                    task2AcquiredLock = true;
                }
            }
            catch(WalletLockTimeoutException e)
            {
                blockedTaskException = e;
            }
        }); 
        
        await Task.WhenAll(task1, task2);
        
        //A
        Assert.True(task2AcquiredLock);
        Assert.Null(blockedTaskException);
    }
}