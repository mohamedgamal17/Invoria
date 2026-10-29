using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Financial.Domain.Payables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Financial.Infrastructure.EntityFramework.Configuration;

public sealed class PayableSettlementEntityTypeConfiguration : IEntityTypeConfiguration<PayableSettlement>
{
    public void Configure(EntityTypeBuilder<PayableSettlement> builder)
    {
        builder.ToTable(PayableSettlementTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(PayableSettlementTableConsts.IdMaxLength);

        builder.Property(x => x.PayableId)
            .HasMaxLength(PayableSettlementTableConsts.PayableIdMaxLength);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.SettledAt);

        builder.MapAudited();

        builder.HasIndex(x => x.PayableId);
    }
}
