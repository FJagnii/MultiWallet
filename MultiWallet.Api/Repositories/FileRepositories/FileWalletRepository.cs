using System.Text.Json;
using Microsoft.Extensions.Options;
using MultiWallet.Api.Configuration;
using MultiWallet.Api.Exceptions;
using MultiWallet.Api.Models;

namespace MultiWallet.Api.Repositories.FileRepositories;

public class FileWalletRepository : IWalletRepository
{
    private readonly string _walletsFilePath;
    private List<Wallet> _wallets;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<FileWalletRepository> _logger;

    public FileWalletRepository(IOptions<WalletsFileConfig> config, ILogger<FileWalletRepository> logger)
    {
        _walletsFilePath = config.Value.FilePath;
        _logger = logger;
        _semaphore = new(1, 1);

        if (File.Exists(_walletsFilePath))
        {
            if (!ReadWalletsData())
            {
                _wallets = new();
            }
        }
        else
        {
            _wallets = new List<Wallet>();
            try
            {
                using var fs = File.Create(_walletsFilePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Złapano wyjątek przy próbie stworzenia pliku portfeli");
            }
        }
    }

    public async Task CreateWalletAsync(Wallet wallet)
    {
        await _semaphore.WaitAsync();

        try
        {
            int walletId = _wallets.Count;
            wallet.Id = walletId;

            //tworzymy kopię listy portfeli do zapisu i dodajemy do niej nowy portfel
            var walletsToSave = _wallets.Select(w => w.Clone()).ToList();
            walletsToSave.Add(wallet.Clone());

            //próbujemy zapisać tę kopię
            //jeśli zapis się powiedzie - dopiero wtedy zaktualizujemy lokalny "cache" portfeli 
            await SaveWalletsDataAsync(walletsToSave);
            _wallets = walletsToSave;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<Wallet>> GetWalletsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _wallets.Select(w => w.Clone()).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Wallet> GetWalletAsync(int id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var wallet = _wallets.FirstOrDefault(w => w.Id == id);
            return wallet?.Clone();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateWalletAsync(Wallet updatedWallet)
    {
        await _semaphore.WaitAsync();
        try
        {
            var walletsToSave = _wallets.Select(w => w.Clone()).ToList();
            var walletToUpdate = walletsToSave.FirstOrDefault(w => w.Id == updatedWallet.Id);
            walletToUpdate.Currencies = new(updatedWallet.Currencies);

            await SaveWalletsDataAsync(walletsToSave);
            _wallets = walletsToSave;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool ReadWalletsData()
    {
        try
        {
            string json = File.ReadAllText(_walletsFilePath);
            var wallets = JsonSerializer.Deserialize<List<Wallet>>(json);
            _wallets = wallets ?? new();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Złapano wyjątek przy próbie odczytu pliku portfeli");
            return false;
        }
    }

    private async Task SaveWalletsDataAsync(List<Wallet> wallets)
    {
        string tmpWalletsFilePath = _walletsFilePath + ".tmp";
        try
        {
            //serializujemy i wpierw zapisujemy do pliku tymczasowego, a na koniec atomowo podmieniamy plik tymczasowy na docelowy
            //dzięki temu, jeśli na którymś etapie coś pójdzie nie tak, można złapać wyjątek, a plik portfeli nie zostanie zmieniony
            string json = JsonSerializer.Serialize(wallets, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllTextAsync(tmpWalletsFilePath, json);
            File.Replace(tmpWalletsFilePath, _walletsFilePath, null);
        }
        catch (UnauthorizedAccessException e)
        {
            throw new WalletRepositoryException("Brak dostępu przy zapisie pliku portfeli", e);
        }
        catch (IOException e)
        {
            throw new WalletRepositoryException("Błąd I/O przy zapisie pliku portfeli", e);
        }
        catch (NotSupportedException e)
        {
            throw new WalletRepositoryException("Problem z serializacją lub ścieżką do pliku przy zapisie pliku portfeli", e);
        }
        catch (Exception e)
        {
            throw new WalletRepositoryException("Nieoczekiwany błąd przy zapisie pliku portfeli", e);
        }
        finally
        {
            if (File.Exists(tmpWalletsFilePath))
            {
                File.Delete(tmpWalletsFilePath);
            }
        }
    }
}