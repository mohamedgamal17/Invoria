using Invoria.Reporting.Domain.Orders.OrderPeriodSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class OrderPeriodSummaryEntityTypeConfiguration : IEntityTypeConfiguration<OrderPeriodSummary>
{
    public void Configure(EntityTypeBuilder<OrderPeriodSummary> builder)
    {
        builder.ToTable(OrderPeriodSummaryTableConsts.TableName);

        builder.HasKey(x => new { x.Granularity, x.PeriodKey, x.DateField });

        builder.Property(x => x.Granularity)
            .HasMaxLength(OrderPeriodSummaryTableConsts.GranularityMaxLength)
            .IsRequired();

        builder.Property(x => x.PeriodKey)
            .HasMaxLength(OrderPeriodSummaryTableConsts.PeriodKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.DateField).IsRequired();

        builder.Property(x => x.PeriodStart)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.OrderCount).IsRequired();
        builder.Property(x => x.GrossRevenue).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.NetRevenue).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.CancelledCount).IsRequired();
        builder.Property(x => x.DeliveredCount).IsRequired();

        builder.HasIndex(x => new { x.DateField, x.Granularity, x.PeriodStart });
    }
}
