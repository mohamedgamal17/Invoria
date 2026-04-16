using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public sealed class PurchaseStateHistoryEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseStateHistory>
{
    public void Configure(EntityTypeBuilder<PurchaseStateHistory> builder)
    {
        builder.ToTable(PurchaseStateHistoryTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(PurchaseStateHistoryTableConsts.IdMaxLength);

        builder.Property(x => x.PurchaseOrderId)
            .HasMaxLength(PurchaseStateHistoryTableConsts.PurchaseOrderIdMaxLength);

        builder.Property(x => x.FromState);
        builder.Property(x => x.ToState);
        builder.Property(x => x.ChangedAt);

        builder.Property(x => x.Reason)
            .HasMaxLength(PurchaseStateHistoryTableConsts.ReasonMaxLength);

        builder.HasIndex(x => x.PurchaseOrderId);
    }
}
