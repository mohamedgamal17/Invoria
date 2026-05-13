using Invoria.Reporting.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class ReportedOrderStateTransitionEntityTypeConfiguration : IEntityTypeConfiguration<ReportedOrderStateTransition>
{
    public void Configure(EntityTypeBuilder<ReportedOrderStateTransition> builder)
    {
        builder.ToTable(ReportedOrderStateTransitionTableConsts.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(ReportedOrderStateTransitionTableConsts.IdMaxLength);

        builder.Property(x => x.ReportedOrderId)
            .HasMaxLength(ReportedOrderStateTransitionTableConsts.ReportedOrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.FromStatus).IsRequired();
        builder.Property(x => x.ToStatus).IsRequired();
        builder.Property(x => x.FromFullfillmentStatus).IsRequired();
        builder.Property(x => x.ToFullfillmentStatus).IsRequired();
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(ReportedOrderStateTransitionTableConsts.ReasonMaxLength);

        builder.HasIndex(x => x.ReportedOrderId);
        builder.HasIndex(x => x.ChangedAt);
    }
}
