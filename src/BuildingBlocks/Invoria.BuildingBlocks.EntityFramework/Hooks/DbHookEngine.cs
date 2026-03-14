using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public sealed class DbHookEngine : IDbHookEngine
{
    private readonly IEnumerable<IBeforeDbHookSave> _beforeSaveHooks;
    private readonly IEnumerable<IAfterDbHookSave> _afterSaveHooks;
    private readonly ILogger<DbHookEngine>? _logger;

    public DbHookEngine(
        IEnumerable<IBeforeDbHookSave> beforeSaveHooks,
        IEnumerable<IAfterDbHookSave> afterSaveHooks,
        ILogger<DbHookEngine>? logger = null)
    {
        _beforeSaveHooks = beforeSaveHooks;
        _afterSaveHooks = afterSaveHooks;
        _logger = logger;
    }

    public async Task RunBeforeSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        foreach (var hook in _beforeSaveHooks)
        {
            try
            {
                await hook.OnBeforeSaveAsync(dbContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing before-save hook {HookType}", hook.GetType().Name);
                throw;
            }
        }
    }

    public async Task RunAfterSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        foreach (var hook in _afterSaveHooks)
        {
            try
            {
                await hook.OnAfterSaveAsync(dbContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing after-save hook {HookType}", hook.GetType().Name);
                throw;
            }
        }
    }
}

