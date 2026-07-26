using System.Text.Json;

namespace Invoria.BackgroundJob.Core.Checkpoints;

public class JobCheckpoint
{
    public required JobId JobId { get; init; }

    public required string Name { get; init; }

    public string State { get; private set; } = "{}";

    public DateTimeOffset CreatedAt { get; init; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public T? GetState<T>() where T : class
    {
        return JsonSerializer.Deserialize<T>(State);
    }

    public void SetState<T>(T state) where T : class
    {
        State = JsonSerializer.Serialize(state);
    }
}
