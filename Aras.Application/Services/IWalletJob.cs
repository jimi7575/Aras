namespace Aras.Services;

public interface IWalletJob
{
    Task ApplyPendingOrdersAsync(CancellationToken cancellationToken = default);
}
