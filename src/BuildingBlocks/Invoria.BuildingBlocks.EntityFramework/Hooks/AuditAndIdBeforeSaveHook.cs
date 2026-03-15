using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public class AuditAndIdBeforeSaveHook : IBeforeDbHookSave
{
    public Task OnBeforeSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
        {
            if (entry.State == EntityState.Added)
            {
                HandleCreatedAudit(entry);
            }
            else if (entry.State == EntityState.Modified)
            {
                HandleModifiedAudit(entry);
            }
        }

        return Task.CompletedTask;
    }

    private static void HandleCreatedAudit(EntityEntry entry)
    {
        if (entry.Entity is not IAuditedEntity auditedEntity)
        {
            return;
        }

        if (auditedEntity.CreatedAt == default)
        {
            // CreatedAt has protected setter, so use EF property access
            entry.Property(nameof(IAuditedEntity.CreatedAt)).CurrentValue = DateTimeOffset.UtcNow;
        }
    }

    private static void HandleModifiedAudit(EntityEntry entry)
    {
        if (entry.Entity is not IAuditedEntity)
        {
            return;
        }

        entry.Property(nameof(IAuditedEntity.LastModifiedAt)).CurrentValue = DateTimeOffset.UtcNow;
    }
}

