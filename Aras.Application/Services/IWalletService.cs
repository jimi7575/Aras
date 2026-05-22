using Aras.Contracts;

namespace Aras.Services;

public interface IWalletService
{
    Task<WalletResponse?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
}
