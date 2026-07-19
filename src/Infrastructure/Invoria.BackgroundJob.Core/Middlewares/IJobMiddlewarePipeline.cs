using Invoria.BackgroundJob.Core.Context;

namespace Invoria.BackgroundJob.Core.Middlewares;

public interface IJobMiddlewarePipeline
{
    Task ExecuteAsync(
        IJobExecutionContext context,
        JobExecutionDelegate terminal);
}
