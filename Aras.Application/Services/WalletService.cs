using Aras.Contracts;
using Aras.Application.Abstractions;

namespace Aras.Services;

public sealed class WalletService(IWalletRepository wallets) : IWalletService
{
    public async Task<WalletResponse?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken)
    {
        var wallet = await wallets.GetByCustomerIdAsync(customerId, cancellationToken);
        return wallet is null ? null : new WalletResponse(wallet.CustomerId, wallet.Balance, wallet.UpdatedAtUtc);
    }
}
