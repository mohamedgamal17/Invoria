using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration
{
    public class OrderStateTransitionHistoryEntityTypeConfiguration : IEntityTypeConfiguration<OrderStateTransitionHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStateTransitionHistory> builder)
        {
            builder.ToTable(OrderStateTransitionHistoryTableConsts.TableName);

            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(OrderStateTransitionHistoryTableConsts.IdMaxLength);

            builder.Property(x => x.OrderId)
                .HasMaxLength(OrderStateTransitionHistoryTableConsts.OrderIdMaxLength);

            builder.Property(x => x.FromStatus);

            builder.Property(x => x.ToStatus);

            builder.Property(x => x.ChangedAt);

            builder.HasIndex(x => x.OrderId);

            builder.HasIndex(x => x.ChangedAt);
        }
    }
}