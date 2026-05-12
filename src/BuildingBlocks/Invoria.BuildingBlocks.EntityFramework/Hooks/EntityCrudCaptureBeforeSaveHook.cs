using Invoria.BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public sealed class EntityCrudCaptureBeforeSaveHook : IBeforeDbHookSave
{
    private readonly IEntityCrudChangeAccumulator _accumulator;

    public EntityCrudCaptureBeforeSaveHook(IEntityCrudChangeAccumulator accumulator)
    {
        _accumulator = accumulator;
    }

    public Task OnBeforeSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        _accumulator.StartBatch();

        foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is not IAggregateRoot)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    _accumulator.Enqueue(entry.Entity, EntityCrudKind.Created);
                    break;
                case EntityState.Modified:
                    _accumulator.Enqueue(entry.Entity, EntityCrudKind.Updated);
                    break;
                case EntityState.Deleted:
                    _accumulator.Enqueue(entry.Entity, EntityCrudKind.Deleted);
                    break;
            }
        }

        return Task.CompletedTask;
    }
}
