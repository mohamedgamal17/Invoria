using Hangfire;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Jobs;

namespace Invoria.BackgroundJobs.Hangfire;

public sealed class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _client;

    public HangfireJobScheduler(IBackgroundJobClient client)
    {
        _client = client;
    }

    public string Enqueue<TJob>()
        where TJob : class, IJob
    {
        string jobId = _client.Enqueue<TJob>(job => job.Execute(CancellationToken.None));
        return jobId;
    }

    public string Schedule<TJob>(TimeSpan delay)
        where TJob : class, IJob
    {
        string jobId = _client.Schedule<TJob>(job => job.Execute(CancellationToken.None), delay);
        return jobId;
    }

    public bool Delete(string jobId)
    {
        bool result = _client.Delete(jobId);
        return result;
    }
}
