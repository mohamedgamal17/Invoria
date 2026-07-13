using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJob.Core.Scheduling;

namespace Invoria.BackgroundJob.Core;

public interface IRecurringJobScheduler
{
    void AddOrUpdate<TJob>(
        string recurringJobId,
        Recurrence recurrence)
        where TJob : class, IJob;

    void Remove(string recurringJobId);

    void Trigger(string recurringJobId);
}
