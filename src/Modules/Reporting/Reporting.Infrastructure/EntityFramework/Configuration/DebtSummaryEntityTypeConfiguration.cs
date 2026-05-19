using Invoria.Reporting.Domain.Orders.DebtSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class DebtSummaryEntityTypeConfiguration : IEntityTypeConfiguration<DebtSummaryBase>
{
    public void Configure(EntityTypeBuilder<DebtSummaryBase> builder)
    {
        builder.ToTable(DebtSummaryTableConsts.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(DebtSummaryTableConsts.IdMaxLength)
            .IsRequired();

        builder.HasDiscriminator(x => x.SummaryType)
            .HasValue<DebtGlobalSummary>(DebtSummaryType.Global)
            .HasValue<DebtCustomerSummary>(DebtSummaryType.Customer);

        builder.Property(x => x.SummaryType).IsRequired();

        builder.Property(x => x.TotalOutstanding).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalPaid).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalOrderValue).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DebtOrderCount).IsRequired();
        builder.Property(x => x.PartiallyPaidCount).IsRequired();
        builder.Property(x => x.UnpaidCount).IsRequired();
        builder.Property(x => x.ComputedAt).IsRequired();
    }
}
