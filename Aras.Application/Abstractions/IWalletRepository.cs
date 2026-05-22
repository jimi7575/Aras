using Aras.Domain;

namespace Aras.Application.Abstractions;

public interface IWalletRepository
{
    Task<Wallet?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
    Task<bool> TryApplyBalanceAsync(int walletId, decimal signedAmount, CancellationToken cancellationToken);
    void AddTransaction(WalletTransaction transaction);
}
