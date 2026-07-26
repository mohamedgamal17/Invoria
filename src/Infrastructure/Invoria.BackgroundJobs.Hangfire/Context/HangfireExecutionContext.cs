using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Context;

namespace Invoria.BackgroundJobs.Hangfire;

internal sealed class HangfireExecutionContext
    : IJobExecutionContext
{
    public JobId JobId { get; }

    public CancellationToken CancellationToken { get; }

    public HangfireExecutionContext(
        JobId jobId,
        CancellationToken cancellationToken)
    {
        JobId = jobId;
        CancellationToken = cancellationToken;
    }
}
