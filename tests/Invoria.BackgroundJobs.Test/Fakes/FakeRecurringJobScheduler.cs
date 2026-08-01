using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJob.Core.Scheduling;

namespace Invoria.BackgroundJobs.Test.Fakes;

public sealed class FakeRecurringJobScheduler : IRecurringJobScheduler
{
    private readonly List<string> _recurringJobIds = new();

    public IReadOnlyList<string> RecurringJobIds => _recurringJobIds;

    public void AddOrUpdate<TJob>(string recurringJobId, Recurrence recurrence)
        where TJob : class, IJob
    {
        if (!_recurringJobIds.Contains(recurringJobId))
        {
            _recurringJobIds.Add(recurringJobId);
        }
    }

    public void Remove(string recurringJobId)
    {
        _recurringJobIds.Remove(recurringJobId);
    }

    public void Trigger(string recurringJobId)
    {
    }
}
