using Aras.Application.Abstractions;
using Aras.Domain;
using Microsoft.Extensions.Logging;

namespace Aras.Services;

public sealed class WalletJob(
    IOrderRepository orders,
    IWalletRepository wallets,
    IUnitOfWork unitOfWork,
    ILogger<WalletJob> logger) : IWalletJob
{
    public async Task ApplyPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orderIds = await orders.GetPendingIdsAsync(100, cancellationToken);

        foreach (var orderId in orderIds)
        {
            await ApplyOrderAsync(orderId, cancellationToken);
        }
    }

    private async Task ApplyOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var order = await orders.GetPendingWithTransactionAsync(orderId, cancellationToken);

        if (order is null || order.Status != OrderStatus.Pending || order.WalletTransaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var wallet = await wallets.GetByCustomerIdAsync(order.CustomerId, cancellationToken);
        if (wallet is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        var signedAmount = order.Side == OrderSide.Buy ? -order.Amount : order.Amount;
        if (wallet.Balance + signedAmount < 0)
        {
            order.Status = OrderStatus.Rejected;
            order.UpdatedAtUtc = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        wallet.Balance += signedAmount;
        wallet.UpdatedAtUtc = DateTime.UtcNow;

        order.Status = OrderStatus.Applied;
        order.AppliedAtUtc = DateTime.UtcNow;

        wallets.AddTransaction(new WalletTransaction
        {
            WalletId = wallet.Id,
            OrderId = order.Id,
            Amount = Math.Abs(order.Amount),
            Type = order.Side == OrderSide.Buy ? WalletTransactionType.Debit : WalletTransactionType.Credit
        });

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Order {OrderId} was probably applied by another worker or a concurrent update happened.", orderId);
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}
