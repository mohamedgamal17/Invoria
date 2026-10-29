using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Financial.Domain.Receivables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Financial.Infrastructure.EntityFramework.Configuration;

public sealed class ReceivableSettlementEntityTypeConfiguration : IEntityTypeConfiguration<ReceivableSettlement>
{
    public void Configure(EntityTypeBuilder<ReceivableSettlement> builder)
    {
        builder.ToTable(ReceivableSettlementTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReceivableSettlementTableConsts.IdMaxLength);

        builder.Property(x => x.ReceivableId)
            .HasMaxLength(ReceivableSettlementTableConsts.ReceivableIdMaxLength);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.SettledAt);

        builder.MapAudited();

        builder.HasIndex(x => x.ReceivableId);
    }
}
