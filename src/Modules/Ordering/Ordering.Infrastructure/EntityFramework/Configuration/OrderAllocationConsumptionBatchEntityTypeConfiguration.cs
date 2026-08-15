using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class OrderAllocationConsumptionBatchEntityTypeConfiguration : IEntityTypeConfiguration<OrderAllocationConsumptionBatch>
{
    public void Configure(EntityTypeBuilder<OrderAllocationConsumptionBatch> builder)
    {
        builder.ToTable(OrderAllocationConsumptionBatchTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(OrderAllocationConsumptionBatchTableConsts.IdMaxLength);

        builder.Property(x => x.ConsumptionLineId)
            .HasMaxLength(OrderAllocationConsumptionBatchTableConsts.ConsumptionLineIdMaxLength)
            .IsRequired();

        builder.Property(x => x.BatchId)
            .HasMaxLength(OrderAllocationConsumptionBatchTableConsts.BatchIdMaxLength)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.MapAudited();

        builder.HasIndex(x => x.ConsumptionLineId);
        builder.HasIndex(x => x.BatchId);
    }
}
