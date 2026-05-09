using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class OrderPaymentEntityTypeConfiguration : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(OrderPaymentTableConsts.IdMaxLength);

        builder.Property(x => x.OrderId)
            .HasMaxLength(OrderPaymentTableConsts.OrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.PaidAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PaymentMethod)
            .IsRequired();

        builder.Property(x => x.PaidAt)
            .IsRequired();

        builder.MapAudited();

        builder.HasIndex(x => x.OrderId);
    }
}
