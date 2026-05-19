using Invoria.Reporting.Domain.Orders.DebtSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class DebtGlobalSummaryEntityTypeConfiguration : IEntityTypeConfiguration<DebtGlobalSummary>
{
    public void Configure(EntityTypeBuilder<DebtGlobalSummary> builder)
    {
        builder.Ignore(x => x.CollectionRate);
    }
}
