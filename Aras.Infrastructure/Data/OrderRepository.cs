using Aras.Application.Abstractions;
using Aras.Contracts;
using Aras.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aras.Infrastructure.Data;

public sealed class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Order?> GetPendingWithTransactionAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.Orders
            .Include(x => x.WalletTransaction)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status == OrderStatus.Pending, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetPendingIdsAsync(int take, CancellationToken cancellationToken)
    {
        return await db.Orders
            .Where(x => x.Status == OrderStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.Orders
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new OrderResponse(
                x.Id,
                x.CustomerId,
                x.Side,
                x.Amount,
                x.Description,
                x.Status,
                x.CreatedAtUtc,
                x.AppliedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        db.Orders.Add(order);
    }
}
