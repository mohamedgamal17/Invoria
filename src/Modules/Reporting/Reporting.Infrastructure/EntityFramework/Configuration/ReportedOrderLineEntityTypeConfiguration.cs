using Invoria.Reporting.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class ReportedOrderLineEntityTypeConfiguration : IEntityTypeConfiguration<ReportedOrderLine>
{
    public void Configure(EntityTypeBuilder<ReportedOrderLine> builder)
    {
        builder.ToTable(ReportedOrderLineTableConsts.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(ReportedOrderLineTableConsts.IdMaxLength);

        builder.Property(x => x.ReportedOrderId)
            .HasMaxLength(ReportedOrderLineTableConsts.ReportedOrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasMaxLength(ReportedOrderLineTableConsts.ProductIdMaxLength)
            .IsRequired();

        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.ReportedOrderId);
        builder.HasIndex(x => x.ProductId);
    }
}
