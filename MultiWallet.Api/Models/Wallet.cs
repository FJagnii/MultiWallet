namespace MultiWallet.Api.Models;

public class Wallet
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<CurrencyData> Currencies { get; set; } = new();

    public Wallet Clone()
    {
        return new Wallet()
        {
            Id = this.Id,
            Name = this.Name,
            Currencies = this.Currencies.Select(c => new CurrencyData()
            {
                Name = c.Name,
                Code = c.Code,
                Amount = c.Amount
            }).ToList()
        };
    }
}

public class CurrencyData
{
    public string Name { get; set; }
    public string Code { get; set; }

    private decimal _amount;

    public decimal Amount
    {
        get => _amount;
        set => _amount = Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}