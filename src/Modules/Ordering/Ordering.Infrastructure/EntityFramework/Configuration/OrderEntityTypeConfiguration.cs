using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration
{
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(OrderTableConsts.IdMaxLength);

            builder.Property(x => x.OrderNumber)
                .HasMaxLength(OrderTableConsts.OrderNumberMaxLength);

            builder.Property(x => x.CustomerId)
                .HasMaxLength(OrderTableConsts.CustomerIdMaxLength);

            builder.Property(x => x.Status);

            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.MapAudited();

            builder.HasIndex(x => x.CustomerId);
        }
    }
}
