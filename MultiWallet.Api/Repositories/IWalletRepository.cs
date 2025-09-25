using MultiWallet.Api.Models;

namespace MultiWallet.Api.Repositories;

public interface IWalletRepository
{
    Task CreateWalletAsync(Wallet wallet);
    Task<List<Wallet>> GetWalletsAsync();
    Task<Wallet> GetWalletAsync(int id);
    Task UpdateWalletAsync(Wallet wallet);
}