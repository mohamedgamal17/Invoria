using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public class ReportPurchaseOrdersCompletedMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportPurchaseOrdersCompletedMetrics>
{
    public void Configure(EntityTypeBuilder<ReportPurchaseOrdersCompletedMetrics> builder)
    {
        builder.ToTable(ReportPurchaseOrdersCompletedMetricsTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReportPurchaseOrdersCompletedMetricsTableConsts.IdMaxLength);

        builder.Property(x => x.Date);
        builder.Property(x => x.TotalCount);
        builder.Property(x => x.Period);

        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
    }
}