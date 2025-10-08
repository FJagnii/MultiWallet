using MultiWallet.Api.Services.WalletOperationResponses;

namespace MultiWallet.Api.Services;

public interface IWalletTransactionService
{
    /// <summary>
    /// Zasila portfel środkami w podanej walucie
    /// </summary>
    /// <param name="walletId">Identyfikator portfela, który ma być zasilony</param>
    /// <param name="currencyCode">Kod waluty, która ma zostać zasilona</param>
    /// <param name="amount">Wartość środków, która ma zostać zasilona</param>
    /// <returns>Kod statusowy i, w przypadku sukcesu, wartość środków w podanej walucie po operacji</returns>
    Task<AddFundsResponse> AddFundsAsync(int walletId, string currencyCode, decimal amount);

    /// <summary>
    /// Pobiera środki z portfela w podanej walucie
    /// </summary>
    /// <param name="walletId">Identyfikator portfela, z którego mają zostać pobrane środki</param>
    /// <param name="currencyCode">Kod waluty, w której mają zostać pobrane środki</param>
    /// <param name="amount">Wartość środków, która ma zostać pobrana</param>
    /// <returns>Kod statusowy i, w przypadku sukcesu, wartość środków w podanej walucie po operacji</returns>
    Task<WithdrawFundsResponse> WithdrawFundsAsync(int walletId, string currencyCode, decimal amount);

    /// <summary>
    /// Wymienia środki w podanej kwocie z waluty źródłowej na walutę docelową
    /// </summary>
    /// <param name="walletId">Identyfikator portfela, w którym mają zostać wymienione środki</param>
    /// <param name="sourceCurrencyCode">Kod waluty, z której mają zostać wymienione środki</param>
    /// <param name="targetCurrencyCode">Kod waluty, w której mają być wymienione środki</param>
    /// <param name="amount">Wartość środków, z których ma nastąpić wymiana</param>
    /// <returns>Kod statusowy i, w przypadku sukcesu, wartość środków w walucie źródłowej i walucie docelowej po operacji</returns>
    Task<ExchangeFundsResponse> ExchangeFromFundsAsync(int walletId, string sourceCurrencyCode, string targetCurrencyCode,
        decimal amount);

    /// <summary>
    /// Wymienia środki z waluty źródłowej na podaną kwotę w walucie docelowej
    /// </summary>
    /// <param name="walletId">Identyfikator portfela, w którym mają zostać wymienione środki</param>
    /// <param name="sourceCurrencyCode">Kod waluty, z której mają zostać wymienione środki</param>
    /// <param name="targetCurrencyCode">Kod waluty, w której mają być wymienione środki</param>
    /// <param name="amount">Docelowa wartość środków po wymianie</param>
    /// <returns>Kod statusowy i, w przypadku sukcesu, wartość środków w walucie źródłowej i walucie docelowej po operacji</returns>
    Task<ExchangeFundsResponse> ExchangeToFundsAsync(int walletId, string sourceCurrencyCode, string targetCurrencyCode,
        decimal amount);
}