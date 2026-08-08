using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration
{
    public class ReportOrderSalesMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportOrderSalesMetrics>
    {
        public void Configure(EntityTypeBuilder<ReportOrderSalesMetrics> builder)
        {
            builder.ToTable(ReportOrderSalesMetricsTableConsts.TableName);

            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(ReportOrderSalesMetricsTableConsts.IdMaxLength);

            builder.Property(x => x.Date);
            builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.TotalNetAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.TotalReturnAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Period);

            builder.HasIndex(x => x.Date);
            builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
        }
    }
}