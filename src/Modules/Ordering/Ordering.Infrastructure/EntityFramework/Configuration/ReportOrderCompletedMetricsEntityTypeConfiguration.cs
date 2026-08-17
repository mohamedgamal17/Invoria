using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration
{
    public class ReportOrderCompletedMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportOrderCompletedMetrics>
    {
        public void Configure(EntityTypeBuilder<ReportOrderCompletedMetrics> builder)
        {
            builder.ToTable(ReportOrderCompletedMetricsTableConsts.TableName);

            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(ReportOrderCompletedMetricsTableConsts.IdMaxLength);

            builder.Property(x => x.Date);
            builder.Property(x => x.TotalCount);
            builder.Property(x => x.Period);

            builder.HasIndex(x => x.Date);
            builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
        }
    }
}
