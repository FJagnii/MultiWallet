using MultiWallet.Api.DTOs;

namespace MultiWallet.Api.Clients;

public interface INbpCommunicationClient
{
    Task<NbpExchangeRatesTableDto> GetExchangeRatesTableAsync();
}