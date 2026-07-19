using Invoria.BackgroundJob.Core;

namespace Invoria.BackgroundJob.Core.Context;

public interface IJobExecutionContext
{
    JobId JobId { get; }

    CancellationToken CancellationToken { get; }
}
