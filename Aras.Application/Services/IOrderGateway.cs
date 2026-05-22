using Aras.Contracts;

namespace Aras.Services;

public interface IOrderGateway
{
    Task<OrderResponse> AddOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken);
    Task<OrderResponse> EditOrderAsync(Guid id, OrderEditRequest request, CancellationToken cancellationToken);
}
