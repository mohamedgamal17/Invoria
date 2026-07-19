namespace Invoria.BackgroundJob.Core.Checkpoints;

public class JobCheckpoint<TState>
{
    public required JobId JobId { get; init; }

    public required string StateName { get; init; }

    public required TState State { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; }
        = DateTimeOffset.UtcNow;
}
