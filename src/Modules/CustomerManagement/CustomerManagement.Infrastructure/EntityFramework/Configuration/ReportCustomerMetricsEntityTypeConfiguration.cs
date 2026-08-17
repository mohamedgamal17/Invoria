using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.CustomerManagement.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.CustomerManagement.Infrastructure.EntityFramework.Configuration
{
    public class ReportCustomerMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportCustomerMetrics>
    {
        public void Configure(EntityTypeBuilder<ReportCustomerMetrics> builder)
        {
            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(ReportCustomerMetricsTableConsts.IdMaxLength);

            builder.Property(x => x.Date);
            builder.Property(x => x.TotalCount);
            builder.Property(x => x.Period);

            builder.HasIndex(x => x.Date);
            builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
        }
    }
}
