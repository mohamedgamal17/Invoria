using System.Text.Json;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public sealed class PurchaseOrderItemEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

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
        builder.Ignore(x => x.CreatedBatchIds);

        builder.Property<List<string>>("_createdBatchIds")
            .HasColumnName("CreatedBatchIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => string.IsNullOrEmpty(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(
                new ValueComparer<List<string>>(
                    (a, b) => ReferenceEquals(a, b) || (a != null && b != null && a.SequenceEqual(b)),
                    v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())),
                    v => v.ToList()));

        builder.HasIndex(x => x.PurchaseOrderId);
        builder.HasIndex(x => x.ProductId);
    }
}
