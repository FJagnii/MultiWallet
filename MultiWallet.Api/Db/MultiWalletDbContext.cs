using Microsoft.EntityFrameworkCore;
using MultiWallet.Api.Models;

namespace MultiWallet.Api.Db;

public class MultiWalletDbContext : DbContext
{
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<NbpCurrencyData> NbpCurrencies { get; set; }

    public MultiWalletDbContext(DbContextOptions<MultiWalletDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>().OwnsMany(w => w.Currencies, cb =>
        {
            cb.WithOwner().HasForeignKey("WalletId"); //wskazanie "wprost" jak ma się nazywać klucz obcy
            cb.HasKey("WalletId", "Code"); //klucz złożony z id portfela i kodu waluty

            cb.Property(c => c.Name).IsRequired();
            cb.Property(c => c.Code).IsRequired();
            cb.Property(c => c.Amount).IsRequired().HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<NbpCurrencyData>().HasKey(c => c.Code); //oznaczenie kodu waluty z NBP jako PK

        base.OnModelCreating(modelBuilder);
    }
}