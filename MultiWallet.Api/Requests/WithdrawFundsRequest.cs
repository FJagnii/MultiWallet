using System.ComponentModel.DataAnnotations;

namespace MultiWallet.Api.Requests;

public class WithdrawFundsRequest
{
    private string _currencyCode;

    [Required]
    public string CurrencyCode
    {
        get => _currencyCode;
        set => _currencyCode = value?.ToUpper() ?? throw new ArgumentNullException(nameof(value));
    }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Kwota musi być większa od zera")]
    public decimal Amount { get; set; }
}