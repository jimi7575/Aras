using Aras.Contracts;

namespace Aras.Services;

public interface IOrderService
{
    Task<OrderResponse> AddOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken);
    Task<OrderResponse> EditOrderAsync(Guid id, OrderEditRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken);
}
