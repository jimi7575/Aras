using Aras.Contracts;
using Aras.Application.Abstractions;
using Aras.Domain;

namespace Aras.Services;

public sealed class OrderService(
    ICustomerRepository customers,
    IOrderRepository orders,
    IWalletRepository wallets,
    IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<OrderResponse> AddOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var customerExists = await customers.ExistsAsync(request.CustomerId!.Value, cancellationToken);
        if (!customerExists)
        {
            throw new KeyNotFoundException("Customer was not found.");
        }

        await EnsureSufficientBalanceAsync(request.CustomerId.Value, request.Side!.Value, request.Amount, cancellationToken);

        var order = new Order(
            request.CustomerId!.Value,
            request.Side!.Value,
            request.Amount,
            request.Description);

        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToResponse();
    }

    public async Task<OrderResponse> EditOrderAsync(Guid id, OrderEditRequest request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new KeyNotFoundException("Order was not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be edited.");
        }

        await EnsureSufficientBalanceAsync(order.CustomerId, request.Side!.Value, request.Amount, cancellationToken);

        order.Update(request.Side!.Value, request.Amount, request.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToResponse();
    }

    public async Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        return await orders.GetAllAsync(cancellationToken);
    }

    private async Task EnsureSufficientBalanceAsync(
        int customerId,
        OrderSide side,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (side != OrderSide.Buy)
        {
            return;
        }

        var wallet = await wallets.GetByCustomerIdAsync(customerId, cancellationToken);
        if (wallet is null)
        {
            throw new KeyNotFoundException("Wallet was not found.");
        }

        if (!wallet.HasSufficientBalance(amount))
        {
            throw new InvalidOperationException("Insufficient wallet balance. Sell something first.");
        }
    }
}
