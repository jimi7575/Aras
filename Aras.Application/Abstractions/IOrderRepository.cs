using Aras.Contracts;
using Aras.Domain;

namespace Aras.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Order?> GetPendingWithTransactionAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> GetPendingIdsAsync(int take, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken);
    void Add(Order order);
}
