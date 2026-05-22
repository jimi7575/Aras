namespace Aras.Services;

public interface IWalletJobScheduler
{
    void EnqueueApplyPendingOrders();
}
