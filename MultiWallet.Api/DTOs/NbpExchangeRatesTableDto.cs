namespace MultiWallet.Api.DTOs;

public class NbpExchangeRatesTableDto
{
    public string Table { get; set; }
    public string No { get; set; }
    public string EffectiveDate { get; set; }
    public List<NbpCurrencyRateDto> Rates { get; set; } = new();
}

public class NbpCurrencyRateDto
{
    public string Currency { get; set; }
    public string Code { get; set; }
    public decimal Mid { get; set; }
}