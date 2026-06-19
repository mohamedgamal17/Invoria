using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class InvoiceItemEntityTypeConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable(InvoiceItemTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(InvoiceItemTableConsts.IdMaxLength);

        builder.Property(x => x.OrderItemId)
            .HasMaxLength(InvoiceItemTableConsts.OrderItemIdMaxLength)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasMaxLength(InvoiceItemTableConsts.ProductIdLength)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property<string>("InvoiceId")
            .HasMaxLength(InvoiceTableConsts.IdMaxLength);

        builder.HasIndex(x => x.ProductId);
    }
}
