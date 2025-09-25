using Microsoft.EntityFrameworkCore;
using MultiWallet.Api.Db;
using MultiWallet.Api.Models;

namespace MultiWallet.Api.Repositories.EfRepositories;

public class EfWalletRepository : IWalletRepository
{
    private readonly MultiWalletDbContext _db;

    public EfWalletRepository(MultiWalletDbContext db)
    {
        _db = db;
    }

    public async Task CreateWalletAsync(Wallet wallet)
    {
        _db.Add(wallet);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Wallet>> GetWalletsAsync()
    {
        return await _db.Wallets.ToListAsync();
    }

    public async Task<Wallet> GetWalletAsync(int id)
    {
        return await _db.Wallets.FindAsync(id);
    }

    public async Task UpdateWalletAsync(Wallet wallet)
    {
        _db.Update(wallet);
        await _db.SaveChangesAsync();
    }
}