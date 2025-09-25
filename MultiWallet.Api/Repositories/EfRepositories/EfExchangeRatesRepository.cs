using Microsoft.EntityFrameworkCore;
using MultiWallet.Api.Db;
using MultiWallet.Api.Models;

namespace MultiWallet.Api.Repositories.EfRepositories;

public class EfExchangeRatesRepository : IExchangeRatesRepository
{
    private readonly MultiWalletDbContext _db;

    public EfExchangeRatesRepository(MultiWalletDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task SaveDataAsync(List<NbpCurrencyData> exchangeRates)
    {
        foreach (var nbpCurrency in exchangeRates)
        {
            var currencyInDb = await _db.NbpCurrencies.FindAsync(nbpCurrency.Code);
            if (currencyInDb == null)
            {
                _db.NbpCurrencies.Add(nbpCurrency);
            }
            else
            {
                //zakładam, że nazwa i kod się nie zmienią
                currencyInDb.Mid = nbpCurrency.Mid;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<NbpCurrencyData> GetCurrencyDataAsync(string currencyCode)
    {
        return await _db.NbpCurrencies.FirstOrDefaultAsync(c => c.Code == currencyCode);
    }
}