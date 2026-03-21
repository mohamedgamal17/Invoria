using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration
{
    public class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(OrderItemTableConsts.IdMaxLength);

            builder.Property(x => x.ProductId)
                .HasMaxLength(OrderItemTableConsts.ProductIdLength);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.Price)
                .IsRequired();

            builder.Property<string>("OrderId")
                .HasMaxLength(OrderTableConsts.IdMaxLength);

            builder.HasIndex(x => x.ProductId);
        }
    }
}
