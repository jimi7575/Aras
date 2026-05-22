namespace Aras.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
