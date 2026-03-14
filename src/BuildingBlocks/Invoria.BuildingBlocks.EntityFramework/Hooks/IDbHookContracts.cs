using Microsoft.EntityFrameworkCore;

namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public interface IDbHook
{
}

public interface IBeforeDbHookSave : IDbHook
{
    Task OnBeforeSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default);
}

public interface IAfterDbHookSave : IDbHook
{
    Task OnAfterSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default);
}

public interface IDbHookEngine
{
    Task RunBeforeSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default);

    Task RunAfterSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default);
}

