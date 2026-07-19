using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Middlewares;

namespace Invoria.BackgroundJobs.Hangfire.Middleware;

internal sealed class JobExecutionContextMiddleware
    : IJobMiddleware
{
    private readonly JobExecutionContextAccessor _accessor;

    public JobExecutionContextMiddleware(
        JobExecutionContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public async Task InvokeAsync(
        IJobExecutionContext context,
        JobExecutionDelegate next)
    {
        _accessor.SetCurrent(context);

        try
        {
            await next(context);
        }
        finally
        {
            _accessor.Clear();
        }
    }
}
