using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Middlewares;

namespace Invoria.BackgroundJobs.Hangfire.Middleware;

internal sealed class HangfireJobMiddlewarePipeline
    : IJobMiddlewarePipeline
{
    private readonly IReadOnlyList<IJobMiddleware> _middlewares;

    public HangfireJobMiddlewarePipeline(
        IEnumerable<IJobMiddleware> middlewares)
    {
        _middlewares = middlewares.ToList();
    }

    public Task ExecuteAsync(
        IJobExecutionContext context,
        JobExecutionDelegate terminal)
    {
        return ExecuteAsync(0);

        Task ExecuteAsync(int index)
        {
            if (index == _middlewares.Count)
            {
                return terminal(context);
            }

            return _middlewares[index].InvokeAsync(
                context,
                _ => ExecuteAsync(index + 1));
        }
    }
}
