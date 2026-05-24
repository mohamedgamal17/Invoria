using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public class BatchAllocationEntityTypeConfiguration : IEntityTypeConfiguration<BatchAllocation>
{
    public void Configure(EntityTypeBuilder<BatchAllocation> builder)
    {
        builder.ToTable(BatchAllocationTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(BatchAllocationTableConsts.IdMaxLength);

        builder.Property(x => x.BatchId)
            .HasMaxLength(BatchAllocationTableConsts.BatchIdMaxLength);

        builder.Property(x => x.OrderItemId)
            .HasMaxLength(BatchAllocationTableConsts.OrderItemIdMaxLength);

        builder.Property(x => x.QuantityAllocated);

        builder.Property(x => x.AllocatedAt);

        builder.Property(x => x.AllocationLineId)
            .HasMaxLength(BatchAllocationTableConsts.AllocationLineIdMaxLength);

        builder.MapAudited();

        builder.HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrderItemId);
    }
}
