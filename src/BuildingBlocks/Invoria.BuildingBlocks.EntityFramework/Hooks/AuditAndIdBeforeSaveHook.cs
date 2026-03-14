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
                HandleIdGeneration(entry);
                HandleCreatedAudit(entry);
            }
            else if (entry.State == EntityState.Modified)
            {
                HandleModifiedAudit(entry);
            }
        }

        return Task.CompletedTask;
    }

    private static void HandleIdGeneration(EntityEntry entry)
    {
        var ulid = UlidGenerator.NewUlid();

        var guidIdProperty = entry.Properties.FirstOrDefault(p =>
            string.Equals(p.Metadata.Name, "Id", StringComparison.OrdinalIgnoreCase) &&
            p.Metadata.ClrType == typeof(Guid));

        if (guidIdProperty is { CurrentValue: Guid guid } && guid == Guid.Empty)
        {
            guidIdProperty.CurrentValue = ulid.ToGuid();
        }

        var stringIdProperty = entry.Properties.FirstOrDefault(p =>
            string.Equals(p.Metadata.Name, "Id", StringComparison.OrdinalIgnoreCase) &&
            p.Metadata.ClrType == typeof(string));

        if (stringIdProperty is { CurrentValue: null or "" })
        {
            stringIdProperty.CurrentValue = ulid.ToString();
        }
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

