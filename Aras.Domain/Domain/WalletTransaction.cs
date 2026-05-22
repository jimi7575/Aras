namespace Aras.Domain;

public sealed class WalletTransaction
{
    private WalletTransaction()
    {
    }

    public WalletTransaction(int walletId, Guid orderId, decimal amount, WalletTransactionType type)
    {
        WalletId = walletId;
        OrderId = orderId;
        Amount = amount;
        Type = type;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public int WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public WalletTransactionType Type { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
}
