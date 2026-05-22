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

    public void AddTransaction(WalletTransaction transaction)
    {
        db.WalletTransactions.Add(transaction);
    }
}
