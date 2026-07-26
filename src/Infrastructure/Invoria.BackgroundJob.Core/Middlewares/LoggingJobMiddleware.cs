using Invoria.BackgroundJob.Core.Context;
using Microsoft.Extensions.Logging;

namespace Invoria.BackgroundJob.Core.Middlewares;

public sealed class LoggingJobMiddleware : IJobMiddleware
{
    private readonly ILogger<LoggingJobMiddleware> _logger;

    public LoggingJobMiddleware(
        ILogger<LoggingJobMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(
        IJobExecutionContext context,
        JobExecutionDelegate next)
    {
        _logger.LogInformation(
            "Starting job {JobId}.",
            context.JobId);

        try
        {
            await next(context);

            _logger.LogInformation(
                "Completed job {JobId}.",
                context.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Job {JobId} failed.",
                context.JobId);

            throw;
        }
    }
}
