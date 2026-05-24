using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Allocations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public sealed class AllocationLineEntityTypeConfiguration : IEntityTypeConfiguration<AllocationLine>
{
    public void Configure(EntityTypeBuilder<AllocationLine> builder)
    {
        builder.ToTable(AllocationLineTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(AllocationLineTableConsts.IdMaxLength);

        builder.Property(x => x.AllocationId)
            .HasMaxLength(AllocationLineTableConsts.AllocationIdMaxLength);

        builder.Property(x => x.OrderItemId)
            .HasMaxLength(AllocationLineTableConsts.OrderItemIdMaxLength);

        builder.Property(x => x.ProductId)
            .HasMaxLength(AllocationLineTableConsts.ProductIdMaxLength);

        builder.Property(x => x.QuantityRequested);

        builder.Property(x => x.Status);

        builder.MapAudited();

        builder.HasIndex(x => x.AllocationId);
        builder.HasIndex(x => x.OrderItemId);

        builder.HasMany(x => x.BatchAllocations)
            .WithOne(x => x.AllocationLine)
            .HasForeignKey(x => x.AllocationLineId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
