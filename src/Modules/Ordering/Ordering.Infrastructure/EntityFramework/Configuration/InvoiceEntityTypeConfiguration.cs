using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class InvoiceEntityTypeConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable(InvoiceTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(InvoiceTableConsts.IdMaxLength);

        builder.Property(x => x.InvoiceNumber)
            .HasMaxLength(InvoiceTableConsts.InvoiceNumberMaxLength);

        builder.Property(x => x.CustomerId)
            .HasMaxLength(InvoiceTableConsts.CustomerIdMaxLength)
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasMaxLength(InvoiceTableConsts.OrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.MapAudited();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();
    }
}
