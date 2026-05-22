using Aras.Contracts;

namespace Aras.Services;

public sealed class OrderGateway(IOrderService orderService, IWalletJobScheduler jobs) : IOrderGateway
{
    public async Task<OrderResponse> AddOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var order = await orderService.AddOrderAsync(request, cancellationToken);
        jobs.EnqueueApplyPendingOrders();
        return order;
    }

    public Task<OrderResponse> EditOrderAsync(Guid id, OrderEditRequest request, CancellationToken cancellationToken)
    {
        return orderService.EditOrderAsync(id, request, cancellationToken);
    }
}
