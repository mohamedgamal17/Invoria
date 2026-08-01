using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;

namespace Invoria.BackgroundJobs.Test.Fakes;

public sealed class FakeJobCheckpointStore : IJobCheckpointStore
{
    private readonly Dictionary<JobId, JobCheckpoint> _checkpoints = new();

    public Task SaveAsync(
        JobCheckpoint checkpoint,
        CancellationToken cancellationToken = default)
    {
        _checkpoints[checkpoint.JobId] = checkpoint;

        return Task.CompletedTask;
    }

    public Task<JobCheckpoint?> RestoreAsync(
        JobId jobId,
        CancellationToken cancellationToken = default)
    {
        if (_checkpoints.TryGetValue(jobId, out var checkpoint))
        {
            return Task.FromResult<JobCheckpoint?>(checkpoint);
        }

        var newCheckpoint = new JobCheckpoint
        {
            JobId = jobId,
            Name = jobId.ToString()
        };

        return Task.FromResult<JobCheckpoint?>(newCheckpoint);
    }

    public Task DeleteAsync(
        JobId jobId,
        CancellationToken cancellationToken = default)
    {
        _checkpoints.Remove(jobId);

        return Task.CompletedTask;
    }
}
