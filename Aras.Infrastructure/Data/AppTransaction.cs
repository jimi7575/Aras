using Aras.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Aras.Infrastructure.Data;

public sealed class AppTransaction(IDbContextTransaction transaction) : IAppTransaction
{
    public ValueTask DisposeAsync()
    {
        return transaction.DisposeAsync();
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken)
    {
        return transaction.RollbackAsync(cancellationToken);
    }
}
