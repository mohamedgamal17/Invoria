using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public class ReportPurchaseSalesMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportPurchaseSalesMetrics>
{
    public void Configure(EntityTypeBuilder<ReportPurchaseSalesMetrics> builder)
    {
        builder.ToTable(ReportPurchaseSalesMetricsTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReportPurchaseSalesMetricsTableConsts.IdMaxLength);

        builder.Property(x => x.Date);
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Period);

        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
    }
}