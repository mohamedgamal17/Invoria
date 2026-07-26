using Hangfire;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJobs.Hangfire.Execution;

namespace Invoria.BackgroundJobs.Hangfire;

public sealed class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _client;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireJobScheduler(
        IBackgroundJobClient client,
        IRecurringJobManager recurringJobManager)
    {
        _client = client;
        _recurringJobManager = recurringJobManager;
    }

    public string Enqueue<TJob>()
        where TJob : class, IJob
    {
        string jobTypeName = typeof(TJob).AssemblyQualifiedName
            ?? throw new InvalidOperationException(
                $"Could not resolve AssemblyQualifiedName for {typeof(TJob).FullName}.");

        string result = _client.Enqueue<HangfireJobDispatcher>(
            dispatcher => dispatcher.ExecuteAsync(jobTypeName, CancellationToken.None));

        return result;
    }

    public string Schedule<TJob>(TimeSpan delay)
        where TJob : class, IJob
    {
        string jobTypeName = typeof(TJob).AssemblyQualifiedName
            ?? throw new InvalidOperationException(
                $"Could not resolve AssemblyQualifiedName for {typeof(TJob).FullName}.");

        string result = _client.Schedule<HangfireJobDispatcher>(
            dispatcher => dispatcher.ExecuteAsync(jobTypeName, CancellationToken.None),
            delay);

        return result;
    }

    public bool Delete(string jobId)
    {
        bool result = _client.Delete(jobId);
        return result;
    }

    public void Remove(string jobName)
    {
        _recurringJobManager.RemoveIfExists(jobName);
    }
}
