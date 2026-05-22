namespace Aras.Domain;

public sealed class Order
{
    private Order()
    {
    }

    public Order(int customerId, OrderSide side, decimal amount, string? description)
    {
        CustomerId = customerId;
        Side = side;
        Amount = amount;
        Description = description;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public OrderSide Side { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? AppliedAtUtc { get; private set; }
    public WalletTransaction? WalletTransaction { get; private set; }

    public void Update(OrderSide side, decimal amount, string? description)
    {
        Side = side;
        Amount = amount;
        Description = description;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Apply()
    {
        Status = OrderStatus.Applied;
        AppliedAtUtc = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = OrderStatus.Rejected;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
