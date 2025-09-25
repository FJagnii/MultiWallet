namespace MultiWallet.Api.Db;

public static class DbInitializer
{
    public static void InitializeDb(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MultiWalletDbContext>();
        db.Database.EnsureCreated();
    }
}