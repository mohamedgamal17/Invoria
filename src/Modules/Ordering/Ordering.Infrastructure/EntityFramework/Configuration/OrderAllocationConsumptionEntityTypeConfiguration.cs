using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class OrderAllocationConsumptionEntityTypeConfiguration : IEntityTypeConfiguration<OrderAllocationConsumption>
{
    public void Configure(EntityTypeBuilder<OrderAllocationConsumption> builder)
    {
        builder.ToTable(OrderAllocationConsumptionTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(OrderAllocationConsumptionTableConsts.IdMaxLength);

        builder.Property(x => x.OrderId)
            .HasMaxLength(OrderAllocationConsumptionTableConsts.OrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.AllocationId)
            .HasMaxLength(OrderAllocationConsumptionTableConsts.AllocationIdMaxLength)
            .IsRequired();

        builder.MapAudited();

        builder
            .Navigation(a => a.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.AllocationId)
            .IsUnique();

        builder.HasMany<OrderAllocationConsumptionLine>(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.ConsumptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
