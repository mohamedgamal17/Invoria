using Invoria.Reporting.Domain.Orders.DebtSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class DebtCustomerSummaryEntityTypeConfiguration : IEntityTypeConfiguration<DebtCustomerSummary>
{
    public void Configure(EntityTypeBuilder<DebtCustomerSummary> builder)
    {
        builder.Property(x => x.CustomerId)
            .HasMaxLength(DebtSummaryTableConsts.CustomerIdMaxLength)
            .IsRequired();

        builder.Property(x => x.OldestDebtDate).IsRequired();
        builder.Property(x => x.OldestDebtAmount).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(x => x.CustomerId);
    }
}
