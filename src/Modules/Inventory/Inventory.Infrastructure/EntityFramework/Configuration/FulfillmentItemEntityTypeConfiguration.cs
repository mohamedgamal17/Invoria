using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Fulfillments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public sealed class FulfillmentItemEntityTypeConfiguration : IEntityTypeConfiguration<FulfillmentItem>
{
    public void Configure(EntityTypeBuilder<FulfillmentItem> builder)
    {
        builder.ToTable(FulfillmentItemTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(FulfillmentItemTableConsts.IdMaxLength);

        builder.Property(x => x.FulfillmentId)
            .HasMaxLength(FulfillmentItemTableConsts.FulfillmentIdMaxLength);

        builder.Property(x => x.ProductId)
            .HasMaxLength(FulfillmentItemTableConsts.ProductIdMaxLength);

        builder.Property(x => x.AllocationItemId)
            .HasMaxLength(FulfillmentItemTableConsts.AllocationItemIdMaxLength);

        builder.Property(x => x.AllocatedQuantity);

        builder.MapAudited();

        builder.HasIndex(x => x.FulfillmentId);
        builder.HasIndex(x => x.AllocationItemId);
    }
}
