using Hangfire.Server;

namespace Invoria.BackgroundJobs.Hangfire.Middleware;

internal sealed class HangfireJobIdCaptureFilter : IServerFilter
{
    private static readonly AsyncLocal<string> _currentJobName = new();

    internal static string? CurrentJobName
    {
        get => _currentJobName.Value;
        set
        {
#pragma warning disable CS8601
            _currentJobName.Value = value;
#pragma warning restore CS8601
        }
    }

    public void OnPerforming(PerformingContext context)
    {
        string? jobName = context.GetJobParameter<string>("RecurringJobId");

        if (string.IsNullOrWhiteSpace(jobName))
        {
            string? jobTypeName = context.BackgroundJob.Job.Args[0]?.ToString();

            if (!string.IsNullOrWhiteSpace(jobTypeName))
            {
                jobName = jobTypeName.Split(',')[0].Trim();
            }
        }

        CurrentJobName = jobName;
    }

    public void OnPerformed(PerformedContext context)
    {
        CurrentJobName = null;
    }
}
