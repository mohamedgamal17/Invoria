using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Financial.Domain.Payables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Financial.Infrastructure.EntityFramework.Configuration;

public sealed class PayableEntityTypeConfiguration : IEntityTypeConfiguration<Payable>
{
    public void Configure(EntityTypeBuilder<Payable> builder)
    {
        builder.ToTable(PayableTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(PayableTableConsts.IdMaxLength);

        builder.Property(x => x.PartyId)
            .HasMaxLength(PayableTableConsts.PartyIdMaxLength);

        builder.Property(x => x.SourceId)
            .HasMaxLength(PayableTableConsts.SourceIdMaxLength);

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

        builder.HasMany<PayableSettlement>(x => x.Settlements)
            .WithOne(x => x.Payable)
            .HasForeignKey(x => x.PayableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PartyId);

        builder.HasIndex(x => x.SourceId)
            .IsUnique();
    }
}
