namespace Invoria.BackgroundJob.Core.Checkpoints;

public class JobCheckpoint
{
    public required JobId JobId { get; init; }

    public required string Name { get; init; }

    public required string State { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; }
        = DateTimeOffset.UtcNow;
}
