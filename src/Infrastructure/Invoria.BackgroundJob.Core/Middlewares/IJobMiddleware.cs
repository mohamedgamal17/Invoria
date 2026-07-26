using Invoria.BackgroundJob.Core.Context;

namespace Invoria.BackgroundJob.Core.Middlewares;

public interface IJobMiddleware
{
    Task InvokeAsync(
        IJobExecutionContext context,
        JobExecutionDelegate next);
}
