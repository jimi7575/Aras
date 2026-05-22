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
        var walletUpdated = await wallets.TryApplyBalanceAsync(wallet.Id, signedAmount, cancellationToken);
        if (!walletUpdated)
        {
            order.Reject();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        order.Apply();

        wallets.AddTransaction(new WalletTransaction(
            wallet.Id,
            order.Id,
            Math.Abs(order.Amount),
            order.Side == OrderSide.Buy ? WalletTransactionType.Debit : WalletTransactionType.Credit));

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
