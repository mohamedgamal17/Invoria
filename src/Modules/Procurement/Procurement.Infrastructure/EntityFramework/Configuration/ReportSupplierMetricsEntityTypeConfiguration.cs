using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public class ReportSupplierMetricsEntityTypeConfiguration : IEntityTypeConfiguration<ReportSupplierMetrics>
{
    public void Configure(EntityTypeBuilder<ReportSupplierMetrics> builder)
    {
        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReportSupplierMetricsTableConsts.IdMaxLength);

        builder.Property(x => x.Date);
        builder.Property(x => x.TotalCount);
        builder.Property(x => x.Period);

        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => new { x.Period, x.Date }).IsUnique();
    }
}