using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BuildingBlocks.EntityFramework.Contexts;

public abstract class InvoriaDbContext : DbContext
{
    private readonly IDbHookEngine _dbHookEngine;

    protected InvoriaDbContext(
        DbContextOptions options,
        IDbHookEngine dbHookEngine) : base(options)
    {
        _dbHookEngine = dbHookEngine;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        _dbHookEngine.RunBeforeSaveAsync(this).GetAwaiter().GetResult();

        var result = base.SaveChanges(acceptAllChangesOnSuccess);

        _dbHookEngine.RunAfterSaveAsync(this).GetAwaiter().GetResult();

        return result;
    }

    public override int SaveChanges()
    {
        return SaveChanges(true);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await _dbHookEngine.RunBeforeSaveAsync(this, cancellationToken).ConfigureAwait(false);

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
            .ConfigureAwait(false);

        await _dbHookEngine.RunAfterSaveAsync(this, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(true, cancellationToken);
    }
}

