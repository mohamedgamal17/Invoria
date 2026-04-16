using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public sealed class PurchaseOrderItemEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable(PurchaseOrderItemTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(PurchaseOrderItemTableConsts.IdMaxLength);

        builder.Property(x => x.PurchaseOrderId)
            .HasMaxLength(PurchaseOrderItemTableConsts.PurchaseOrderIdMaxLength);

        builder.Property(x => x.ProductId)
            .HasMaxLength(PurchaseOrderItemTableConsts.ProductIdMaxLength);

        builder.Property(x => x.Quantity);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.SupplierProductCode)
            .HasMaxLength(PurchaseOrderItemTableConsts.SupplierProductCodeMaxLength);

        builder.Ignore(x => x.LineTotal);
        builder.Property<string>("_legacyCreatedBatchIds")
            .HasColumnName("CreatedBatchIds");

        builder.HasIndex(x => x.PurchaseOrderId);
        builder.HasIndex(x => x.ProductId);
    }
}
