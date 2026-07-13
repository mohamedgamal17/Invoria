namespace Invoria.BackgroundJobs.Hangfire;

public sealed class HangfireOptions
{
    public string? ConnectionString { get; set; }

    public string? SchemaName { get; set; }

    public int WorkerCount { get; set; }

    public bool EnableDashboard { get; set; }

    public string DashboardPath { get; set; } = "/jobs";
}
