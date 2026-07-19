using Hangfire.Server;

namespace Invoria.BackgroundJobs.Hangfire.Middleware;

internal sealed class HangfireJobIdCaptureFilter : IServerFilter
{
    private static readonly AsyncLocal<string> _currentJobId = new();

    internal static string? CurrentJobId
    {
        get => _currentJobId.Value;
        set
        {
#pragma warning disable CS8601
            _currentJobId.Value = value;
#pragma warning restore CS8601
        }
    }

    public void OnPerforming(PerformingContext context)
    {
        CurrentJobId = context.BackgroundJob.Id;
    }

    public void OnPerformed(PerformedContext context)
    {
        CurrentJobId = null;
    }
}
