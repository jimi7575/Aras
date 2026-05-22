namespace Aras.Domain;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public OrderSide Side { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? AppliedAtUtc { get; set; }
    public WalletTransaction? WalletTransaction { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
