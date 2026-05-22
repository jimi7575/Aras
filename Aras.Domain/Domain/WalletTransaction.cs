namespace Aras.Domain;

public sealed class WalletTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public decimal Amount { get; set; }
    public WalletTransactionType Type { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
