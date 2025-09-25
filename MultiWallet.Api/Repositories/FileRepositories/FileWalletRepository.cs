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
    private static readonly object _lock = new();
    private readonly ILogger<FileWalletRepository> _logger;

    public FileWalletRepository(IOptions<WalletsFileConfig> config, ILogger<FileWalletRepository> logger)
    {
        _walletsFilePath = config.Value.FilePath;
        _logger = logger;

        lock (_lock)
        {
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
    }

    public Task CreateWalletAsync(Wallet wallet)
    {
        lock (_lock)
        {
            int walletId = _wallets.Count;
            wallet.Id = walletId;

            //tworzymy kopię listy portfeli do zapisu i dodajemy do niej nowy portfel
            var walletsToSave = _wallets.Select(w => w.Clone()).ToList();
            walletsToSave.Add(wallet.Clone());

            //próbujemy zapisać tę kopię
            //jeśli zapis się powiedzie - dopiero wtedy zaktualizujemy lokalny "cache" portfeli 
            SaveWalletsData(walletsToSave);
            _wallets = walletsToSave;
            return Task.CompletedTask;
        }
    }

    public Task<List<Wallet>> GetWalletsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_wallets.Select(w => w.Clone()).ToList());
        }
    }

    public Task<Wallet> GetWalletAsync(int id)
    {
        lock (_lock)
        {
            var wallet = _wallets.FirstOrDefault(w => w.Id == id);
            return Task.FromResult(wallet?.Clone());
        }
    }

    public Task UpdateWalletAsync(Wallet updatedWallet)
    {
        lock (_lock)
        {
            var walletsToSave = _wallets.Select(w => w.Clone()).ToList();
            var walletToUpdate = walletsToSave.FirstOrDefault(w => w.Id == updatedWallet.Id);
            walletToUpdate.Currencies = new(updatedWallet.Currencies);

            SaveWalletsData(walletsToSave);
            _wallets = walletsToSave;
            return Task.CompletedTask;
        }
    }

    private bool ReadWalletsData()
    {
        lock (_lock)
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
    }

    private void SaveWalletsData(List<Wallet> wallets)
    {
        lock (_lock)
        {
            string tmpWalletsFilePath = _walletsFilePath + ".tmp";
            try
            {
                //serializujemy i wpierw zapisujemy do pliku tymczasowego, a na koniec atomowo podmieniamy plik tymczasowy na docelowy
                //dzięki temu, jeśli na którymś etapie coś pójdzie nie tak, można złapać wyjątek, a plik portfeli nie zostanie zmieniony
                string json = JsonSerializer.Serialize(wallets, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(tmpWalletsFilePath, json);
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
}