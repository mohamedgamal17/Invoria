using Invoria.BackgroundJob.Core.Jobs;

namespace Invoria.BackgroundJob.Core;

public interface IJobScheduler
{
    string Enqueue<TJob>()
        where TJob : class, IJob;

    string Schedule<TJob>(TimeSpan delay)
        where TJob : class, IJob;

    bool Delete(string jobId);
}
