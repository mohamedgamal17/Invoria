using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public class ReportOrderSalesProfitMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportOrderSalesProfitMetrics>
{
    public void Configure(EntityTypeBuilder<ReportOrderSalesProfitMetrics> builder)
    {
        builder.ToTable(ReportOrderSalesProfitMetricsTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReportOrderSalesProfitMetricsTableConsts.IdMaxLength);

        builder.Property(x => x.Date);
        builder.Property(x => x.TotalRevenue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalProfit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalReturnAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Period);

        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
    }
}
