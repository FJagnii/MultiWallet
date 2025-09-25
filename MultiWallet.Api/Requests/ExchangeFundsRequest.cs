using System.ComponentModel.DataAnnotations;

namespace MultiWallet.Api.Requests;

public class ExchangeFundsRequest
{
    private string _sourceSourceCurrencyCode;
    private string _targetTargetCurrencyCode;

    [Required]
    public string SourceCurrencyCode
    {
        get => _sourceSourceCurrencyCode;
        set => _sourceSourceCurrencyCode = value?.ToUpper() ?? throw new ArgumentNullException(nameof(value));
    }

    [Required]
    public string TargetCurrencyCode
    {
        get => _targetTargetCurrencyCode;
        set => _targetTargetCurrencyCode = value?.ToUpper() ?? throw new ArgumentNullException(nameof(value));
    }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Kwota musi być większa od zera")]
    public decimal Amount { get; set; }
}