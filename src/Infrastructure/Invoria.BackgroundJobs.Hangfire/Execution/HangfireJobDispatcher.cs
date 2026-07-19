using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJob.Core.Middlewares;
using Invoria.BackgroundJobs.Hangfire.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Hangfire.Execution;

internal sealed class HangfireJobDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IJobMiddlewarePipeline _pipeline;

    public HangfireJobDispatcher(
        IServiceProvider serviceProvider,
        IJobMiddlewarePipeline pipeline)
    {
        _serviceProvider = serviceProvider;
        _pipeline = pipeline;
    }

    public async Task ExecuteAsync<TJob>(
        CancellationToken cancellationToken)
        where TJob : class, IJob
    {
        TJob job = _serviceProvider.GetRequiredService<TJob>();

        string hangfireJobId = HangfireJobIdCaptureFilter.CurrentJobId
            ?? throw new InvalidOperationException(
                "Hangfire job ID was not captured by the server filter.");

        JobId jobId = new(hangfireJobId);

        HangfireExecutionContext context = new(
            jobId,
            cancellationToken);

        await _pipeline.ExecuteAsync(
            context,
            ctx => job.Execute(ctx.CancellationToken));
    }
}
