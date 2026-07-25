using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BackgroundJobs.Hangfire.EntityFramework;

internal sealed class JobCheckpointStore<TContext> : IJobCheckpointStore
    where TContext : DbContext
{
    private readonly TContext _dbContext;

    public JobCheckpointStore(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        JobCheckpoint checkpoint,
        CancellationToken cancellationToken = default)
    {
        JobCheckpoint? existing = await _dbContext
            .Set<JobCheckpoint>()
            .SingleOrDefaultAsync(x => x.JobId == checkpoint.JobId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.State = checkpoint.State;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            await _dbContext.Set<JobCheckpoint>().AddAsync(checkpoint, cancellationToken)
                .ConfigureAwait(false);
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<JobCheckpoint?> RestoreAsync(
        JobId jobId,
        CancellationToken cancellationToken = default)
    {
        JobCheckpoint? result = await _dbContext
            .Set<JobCheckpoint>()
            .SingleOrDefaultAsync(x => x.JobId == jobId, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    public async Task DeleteAsync(
        JobId jobId,
        CancellationToken cancellationToken = default)
    {
        JobCheckpoint? existing = await _dbContext
            .Set<JobCheckpoint>()
            .SingleOrDefaultAsync(x => x.JobId == jobId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            _dbContext.Set<JobCheckpoint>().Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
