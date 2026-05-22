using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class OrderReturnItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderReturnItem>
{
    public void Configure(EntityTypeBuilder<OrderReturnItem> builder)
    {
        builder.ToTable("OrderReturnItems");

        builder.HasKey("OrderId", nameof(OrderReturnItem.OrderItemId));

        builder.Property<string>("OrderId")
            .HasMaxLength(OrderReturnItemTableConsts.OrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.OrderItemId)
            .HasMaxLength(OrderReturnItemTableConsts.OrderItemIdMaxLength)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();
    }
}
