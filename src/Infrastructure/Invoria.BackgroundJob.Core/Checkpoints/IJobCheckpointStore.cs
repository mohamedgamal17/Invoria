namespace Invoria.BackgroundJob.Core.Checkpoints;

public interface IJobCheckpointStore
{
    Task SaveAsync<TState>(
        JobCheckpoint<TState> checkpoint,
        CancellationToken cancellationToken = default);

    Task<JobCheckpoint<TState>?> RestoreAsync<TState>(
        JobId jobId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        JobId jobId,
        CancellationToken cancellationToken = default);
}
