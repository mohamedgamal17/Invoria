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

            builder.Property(x => x.PaymentType)
                .IsRequired();

            builder.Property(x => x.AmountPaid)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.AmountOutstanding)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PaymentStatus)
                .IsRequired();

            builder.Property(x => x.AllocationId)
                .HasMaxLength(OrderTableConsts.AllocationIdMaxLength);

            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Payments)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ReturnItems)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.MapAudited();

            builder.HasIndex(x => x.CustomerId);
        }
    }
}
