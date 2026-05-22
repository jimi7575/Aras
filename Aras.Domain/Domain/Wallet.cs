namespace Aras.Domain;

public sealed class Wallet
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public decimal Balance { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
    public List<WalletTransaction> Transactions { get; set; } = [];
}
