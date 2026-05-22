using Aras.Application.Abstractions;
using Aras.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aras.Infrastructure.Data;

public sealed class WalletRepository(AppDbContext db) : IWalletRepository
{
    public Task<Wallet?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken)
    {
        return db.Wallets.FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public async Task<bool> TryApplyBalanceAsync(int walletId, decimal signedAmount, CancellationToken cancellationToken)
    {
        var updatedRows = await db.Wallets
            .Where(x => x.Id == walletId && x.Balance + signedAmount >= 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Balance, x => x.Balance + signedAmount)
                .SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow),
                cancellationToken);

        return updatedRows == 1;
    }

    public void AddTransaction(WalletTransaction transaction)
    {
        db.WalletTransactions.Add(transaction);
    }
}
