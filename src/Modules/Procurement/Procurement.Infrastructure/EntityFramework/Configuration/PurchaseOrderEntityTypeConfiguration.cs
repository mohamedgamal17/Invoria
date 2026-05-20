using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public sealed class PurchaseOrderEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable(PurchaseOrderTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(PurchaseOrderTableConsts.IdMaxLength);

        builder.Property(x => x.PurchaseNumber)
            .HasMaxLength(PurchaseOrderTableConsts.PurchaseNumberMaxLength);

        builder.Property(x => x.SupplierId)
            .HasMaxLength(PurchaseOrderTableConsts.SupplierIdMaxLength);

        builder.Property(x => x.State);

        builder.Property(x => x.CompletedDate);

        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.MapAudited();


        builder
            .Navigation(o => o.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .Navigation(o => o.StateHistory)
            .HasField("_stateHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SupplierId);

        builder.HasMany<PurchaseOrderItem>(x=> x.Items)
            .WithOne()
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<PurchaseStateHistory>(x=> x.StateHistory)
            .WithOne()
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
