using MultiWallet.Api.Clients;
using MultiWallet.Api.Configuration;
using MultiWallet.Api.Db;
using MultiWallet.Api.Middleware;
using MultiWallet.Api.Repositories;
using MultiWallet.Api.Repositories.EfRepositories;
using MultiWallet.Api.Repositories.FileRepositories;
using MultiWallet.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpClient<INbpCommunicationClient, NbpCommunicationClient>();

bool useEf = builder.Configuration.GetValue<bool>("UseEf");

if (useEf)
{
    builder.Services.AddDbContext<MultiWalletDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("MultiWalletDb"));
    });

    builder.Services.AddScoped<IExchangeRatesRepository, EfExchangeRatesRepository>();
    builder.Services.AddScoped<IWalletRepository, EfWalletRepository>();
}
else
{
    builder.Services.Configure<ExchangeRatesFileConfig>(builder.Configuration.GetSection(nameof(ExchangeRatesFileConfig)));
    builder.Services.AddSingleton<IExchangeRatesRepository, FileExchangeRatesRepository>();

    builder.Services.Configure<WalletsFileConfig>(builder.Configuration.GetSection(nameof(WalletsFileConfig)));
    builder.Services.AddSingleton<IWalletRepository, FileWalletRepository>();
}

builder.Services.AddSingleton<IWalletLockProvider, WalletLockProvider>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddHostedService<NbpExchangeRatesFetcher>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (useEf)
{
    //konfiguracja bazy danych - dla uproszczenia projektu użyte EnsureCreated() zamiast migracji
    DbInitializer.InitializeDb(app);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();