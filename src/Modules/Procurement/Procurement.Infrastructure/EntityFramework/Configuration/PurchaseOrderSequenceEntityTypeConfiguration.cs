using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public sealed class PurchaseOrderSequenceEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseOrderSequence>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderSequence> builder)
    {
        builder.ToTable(PurchaseOrderSequenceTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(PurchaseOrderSequenceTableConsts.IdMaxLength);

        builder.Property(x => x.Year);
        builder.Property(x => x.Month);
        builder.Property(x => x.Day);
        builder.Property(x => x.CurrentValue);

        builder.MapAudited();

        builder.HasIndex(x => new { x.Year, x.Month, x.Day }).IsUnique();
    }
}
