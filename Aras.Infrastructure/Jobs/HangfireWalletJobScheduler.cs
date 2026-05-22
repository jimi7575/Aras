using Aras.Services;
using Hangfire;

namespace Aras.Infrastructure.Jobs;

public sealed class HangfireWalletJobScheduler(IBackgroundJobClient jobs) : IWalletJobScheduler
{
    public void EnqueueApplyPendingOrders()
    {
        jobs.Enqueue<IWalletJob>(job => job.ApplyPendingOrdersAsync(CancellationToken.None));
    }
}
