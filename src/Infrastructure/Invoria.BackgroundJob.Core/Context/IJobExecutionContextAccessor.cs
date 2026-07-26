namespace Invoria.BackgroundJob.Core.Context;

public interface IJobExecutionContextAccessor
{
    IJobExecutionContext? Current { get; }

    JobId? JobId { get; }
}
