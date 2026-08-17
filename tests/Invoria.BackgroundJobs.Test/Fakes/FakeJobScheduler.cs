using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Jobs;

namespace Invoria.BackgroundJobs.Test.Fakes;

public sealed class FakeJobScheduler : IJobScheduler
{
    private readonly List<(string JobType, TimeSpan? Delay)> _scheduledJobs = new();

    public IReadOnlyList<(string JobType, TimeSpan? Delay)> ScheduledJobs => _scheduledJobs;

    public string Enqueue<TJob>()
        where TJob : class, IJob
    {
        _scheduledJobs.Add((typeof(TJob).FullName!, null));

        return Guid.NewGuid().ToString();
    }

    public string Schedule<TJob>(TimeSpan delay)
        where TJob : class, IJob
    {
        _scheduledJobs.Add((typeof(TJob).FullName!, delay));

        return Guid.NewGuid().ToString();
    }

    public bool Delete(string jobId)
    {
        return true;
    }

    public void Remove(string jobName)
    {
    }
}
