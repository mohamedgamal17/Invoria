using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class OrderStateTransitionHistoryEntityTypeConfiguration : IEntityTypeConfiguration<OrderStateTransitionHistory>
{
    public void Configure(EntityTypeBuilder<OrderStateTransitionHistory> builder)
    {
        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(OrderStateTransitionHistoryTableConsts.IdMaxLength);

        builder.Property(x => x.OrderId)
            .HasMaxLength(OrderStateTransitionHistoryTableConsts.OrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.FromStatus)
            .IsRequired();

        builder.Property(x => x.ToStatus)
            .IsRequired();

        builder.Property(x => x.FromFullfillmentStatus)
            .IsRequired();

        builder.Property(x => x.ToFullfillmentStatus)
            .IsRequired();

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(OrderStateTransitionHistoryTableConsts.ReasonMaxLength)
            .IsRequired(false);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ChangedAt);
    }
}
