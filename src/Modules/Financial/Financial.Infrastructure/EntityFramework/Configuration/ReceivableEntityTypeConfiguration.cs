using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Financial.Domain.Receivables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Financial.Infrastructure.EntityFramework.Configuration;

public sealed class ReceivableEntityTypeConfiguration : IEntityTypeConfiguration<Receivable>
{
    public void Configure(EntityTypeBuilder<Receivable> builder)
    {
        builder.ToTable(ReceivableTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReceivableTableConsts.IdMaxLength);

        builder.Property(x => x.PartyId)
            .HasMaxLength(ReceivableTableConsts.PartyIdMaxLength);

        builder.Property(x => x.SourceId)
            .HasMaxLength(ReceivableTableConsts.SourceIdMaxLength);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.OutstandingAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status);

        builder.Ignore(x => x.TotalSettledAmount);

        builder.MapAudited();

        builder
            .Navigation(x => x.Settlements)
            .HasField("_settlements")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany<ReceivableSettlement>(x => x.Settlements)
            .WithOne(x => x.Receivable)
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PartyId);

        builder.HasIndex(x => x.SourceId)
            .IsUnique();
    }
}
