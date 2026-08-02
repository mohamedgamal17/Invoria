using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Catalog.Infrastructure.EntityFramework.Configuration
{
    public class ReportProductMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportProductMetrics>
    {
        public void Configure(EntityTypeBuilder<ReportProductMetrics> builder)
        {
            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(ReportProductMetricsTableConsts.IdMaxLength);

            builder.Property(x => x.Date);
            builder.Property(x => x.TotalCount);
            builder.Property(x => x.Period);

            builder.HasIndex(x => x.Date);
            builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
        }
    }
}
