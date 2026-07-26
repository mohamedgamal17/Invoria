namespace Invoria.BackgroundJobs.Hangfire;

public sealed class HangfireOptions
{
    public string? ConnectionString { get; set; }

    public string? CheckpointsConnectionString { get; set; }

    public string? SchemaName { get; set; }
}
