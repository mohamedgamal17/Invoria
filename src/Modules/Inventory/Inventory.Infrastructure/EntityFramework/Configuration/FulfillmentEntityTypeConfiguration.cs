using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Fulfillments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public sealed class FulfillmentEntityTypeConfiguration : IEntityTypeConfiguration<Fulfillment>
{
    public void Configure(EntityTypeBuilder<Fulfillment> builder)
    {
        builder.ToTable(FulfillmentTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(FulfillmentTableConsts.IdMaxLength);

        builder.Property(x => x.OrderId)
            .HasMaxLength(FulfillmentTableConsts.OrderIdMaxLength);

        builder.Property(x => x.AllocationId)
            .HasMaxLength(FulfillmentTableConsts.AllocationIdMaxLength);

        builder.Property(x => x.Status);

        builder.MapAudited();

        builder
            .Navigation(f => f.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.AllocationId);

        builder.HasMany<FulfillmentItem>(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.FulfillmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
