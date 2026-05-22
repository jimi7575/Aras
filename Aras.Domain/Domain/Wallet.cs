namespace Aras.Domain;

public sealed class Wallet
{
    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public decimal Balance { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;
    public List<WalletTransaction> Transactions { get; private set; } = [];

    public bool HasSufficientBalance(decimal amount)
    {
        return Balance >= amount;
    }
}
