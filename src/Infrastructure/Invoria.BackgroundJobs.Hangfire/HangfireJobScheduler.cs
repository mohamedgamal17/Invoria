using Hangfire;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJobs.Hangfire.Execution;

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
        string result = _client.Enqueue<HangfireJobDispatcher>(
            dispatcher => dispatcher.ExecuteAsync<TJob>(CancellationToken.None));

        return result;
    }

    public string Schedule<TJob>(TimeSpan delay)
        where TJob : class, IJob
    {
        string result = _client.Schedule<HangfireJobDispatcher>(
            dispatcher => dispatcher.ExecuteAsync<TJob>(CancellationToken.None),
            delay);

        return result;
    }

    public bool Delete(string jobId)
    {
        bool result = _client.Delete(jobId);
        return result;
    }
}
