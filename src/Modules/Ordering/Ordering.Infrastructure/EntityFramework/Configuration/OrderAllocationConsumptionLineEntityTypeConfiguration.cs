using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class OrderAllocationConsumptionLineEntityTypeConfiguration : IEntityTypeConfiguration<OrderAllocationConsumptionLine>
{
    public void Configure(EntityTypeBuilder<OrderAllocationConsumptionLine> builder)
    {
        builder.ToTable(OrderAllocationConsumptionLineTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(OrderAllocationConsumptionLineTableConsts.IdMaxLength);

        builder.Property(x => x.ConsumptionId)
            .HasMaxLength(OrderAllocationConsumptionLineTableConsts.ConsumptionIdMaxLength)
            .IsRequired();

        builder.Property(x => x.OrderItemId)
            .HasMaxLength(OrderAllocationConsumptionLineTableConsts.OrderItemIdMaxLength)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasMaxLength(OrderAllocationConsumptionLineTableConsts.ProductIdMaxLength)
            .IsRequired();

        builder.Property(x => x.QuantityRequested)
            .IsRequired();

        builder.MapAudited();

        builder
            .Navigation(x => x.BatchAllocations)
            .AutoInclude();

        builder.HasIndex(x => x.ConsumptionId);
        builder.HasIndex(x => x.OrderItemId);

        builder.HasMany(x => x.BatchAllocations)
            .WithOne()
            .HasForeignKey(x => x.ConsumptionLineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
