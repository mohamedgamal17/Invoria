using Invoria.Reporting.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class ReportedOrderFailureDetailEntityTypeConfiguration : IEntityTypeConfiguration<ReportedOrderFailureDetail>
{
    public void Configure(EntityTypeBuilder<ReportedOrderFailureDetail> builder)
    {
        builder.ToTable(ReportedOrderFailureDetailTableConsts.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(ReportedOrderFailureDetailTableConsts.IdMaxLength);

        builder.Property(x => x.ReportedOrderId)
            .HasMaxLength(ReportedOrderFailureDetailTableConsts.ReportedOrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.ItemId)
            .HasMaxLength(ReportedOrderFailureDetailTableConsts.ItemIdMaxLength)
            .IsRequired();

        builder.Property(x => x.QuantityRequested).IsRequired();
        builder.Property(x => x.QuantityAvailable).IsRequired();
        builder.Property(x => x.Shortage).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.LastModifiedAt);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(256);

        builder.HasIndex(x => x.ReportedOrderId);
        builder.HasIndex(x => x.ItemId);
    }
}
