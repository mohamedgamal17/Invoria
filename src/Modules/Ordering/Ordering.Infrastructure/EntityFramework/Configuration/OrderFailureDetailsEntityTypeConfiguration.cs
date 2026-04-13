using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration;

public sealed class OrderFailureDetailsEntityTypeConfiguration : IEntityTypeConfiguration<OrderFailureDetails>
{
    public void Configure(EntityTypeBuilder<OrderFailureDetails> builder)
    {
        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(OrderFailureDetailsTableConsts.IdMaxLength);

        builder.Property(x => x.ItemId)
            .HasMaxLength(OrderFailureDetailsTableConsts.ItemIdMaxLength)
            .IsRequired();

        builder.Property(x => x.QuantityRequested)
            .IsRequired();

        builder.Property(x => x.QuantityAvailable)
            .IsRequired();

        builder.Property(x => x.Shortage)
            .IsRequired();

        builder.Property<string>("OrderId")
            .HasMaxLength(OrderTableConsts.IdMaxLength);

        builder.MapAudited();

        builder.HasIndex(x => x.ItemId);
    }
}
