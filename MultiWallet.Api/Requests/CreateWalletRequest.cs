using System.ComponentModel.DataAnnotations;

namespace MultiWallet.Api.Requests;

public class CreateWalletRequest
{
    [Required]
    public string WalletName { get; set; }

    private List<string> _currencies;
    public List<string> Currencies
    {
        get => _currencies;
        set => _currencies = value?.Select(c => c.ToUpper()).ToList();
    }
}