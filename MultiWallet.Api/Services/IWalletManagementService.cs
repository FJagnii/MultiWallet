using MultiWallet.Api.Models;

namespace MultiWallet.Api.Services;

public interface IWalletManagementService
{
    /// <summary>
    /// Tworzy nowy wielowalutowy portfel
    /// </summary>
    /// <param name="walletName">Nazwa portfela</param>
    /// <param name="currencyCodes">Opcjonalne: lista kodów walut, które mają znaleźć się w portfelu</param>
    /// <returns>Nowo utworzony portfel</returns>
    Task<Wallet> CreateWalletAsync(string walletName, List<string> currencyCodes = null);

    /// <summary>
    /// Pobiera wszystkie portfele
    /// </summary>
    /// <returns>Lista portfeli</returns>
    Task<List<Wallet>> GetWalletsAsync();

    /// <summary>
    /// Pobiera dany portfel
    /// </summary>
    /// <param name="id">Identyfikator portfela</param>
    /// <returns>Żądany portfel</returns>
    Task<Wallet> GetWalletAsync(int id);
}