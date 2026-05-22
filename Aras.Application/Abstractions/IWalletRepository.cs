using Aras.Domain;

namespace Aras.Application.Abstractions;

public interface IWalletRepository
{
    Task<Wallet?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
    void AddTransaction(WalletTransaction transaction);
}
