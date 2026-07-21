namespace Invoria.BackgroundJob.Core.Checkpoints;

public interface IJobCheckpointStore
{
    Task SaveAsync(
        JobCheckpoint checkpoint,
        CancellationToken cancellationToken = default);

    Task<JobCheckpoint?> RestoreAsync(
        JobId jobId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        JobId jobId,
        CancellationToken cancellationToken = default);
}
