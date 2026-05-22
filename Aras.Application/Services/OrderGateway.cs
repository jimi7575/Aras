using Aras.Contracts;

namespace Aras.Services;

public sealed class OrderGateway(IOrderService orderService) : IOrderGateway
{
    public Task<OrderResponse> AddOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        return orderService.AddOrderAsync(request, cancellationToken);
    }

    public Task<OrderResponse> EditOrderAsync(Guid id, OrderEditRequest request, CancellationToken cancellationToken)
    {
        return orderService.EditOrderAsync(id, request, cancellationToken);
    }
}
