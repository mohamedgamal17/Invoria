using Invoria.Reporting.Domain.Orders.StatusSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class ReportedOrderStatusByDayEntityTypeConfiguration : IEntityTypeConfiguration<ReportedOrderStatusByDay>
{
    public void Configure(EntityTypeBuilder<ReportedOrderStatusByDay> builder)
    {
        builder.ToTable(ReportedOrderStatusByDayTableConsts.TableName);

        builder.HasKey(x => new { x.DayUtc, x.OrderStatus });

        builder.Property(x => x.DayUtc)
            .HasColumnType("date");

        builder.Property(x => x.OrderStatus).IsRequired();

        builder.Property(x => x.Count).IsRequired();
    }
}
