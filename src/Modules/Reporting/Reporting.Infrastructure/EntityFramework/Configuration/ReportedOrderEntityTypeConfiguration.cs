using Invoria.Reporting.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class ReportedOrderEntityTypeConfiguration : IEntityTypeConfiguration<ReportedOrder>
{
    public void Configure(EntityTypeBuilder<ReportedOrder> builder)
    {
        builder.ToTable(ReportedOrderTableConsts.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(ReportedOrderTableConsts.IdMaxLength);

        builder.Property(x => x.OrderNumber)
            .HasMaxLength(ReportedOrderTableConsts.OrderNumberMaxLength)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .HasMaxLength(ReportedOrderTableConsts.CustomerIdMaxLength)
            .IsRequired();

        builder.Property(x => x.OrderStatus).IsRequired();
        builder.Property(x => x.FullfillmentStatus).IsRequired();
        builder.Property(x => x.PaymentType).IsRequired();
        builder.Property(x => x.PaymentStatus).IsRequired();

        builder.Property(x => x.TotalOrderAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AmountOutstanding).HasColumnType("decimal(18,2)");

        builder.Property(x => x.ReplicationVersion).IsRequired();
        builder.Property(x => x.SourceLastKnownAt);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.LastModifiedAt);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(256);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.ReportedOrder)
            .HasForeignKey(x => x.ReportedOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.ReportedOrder)
            .HasForeignKey(x => x.ReportedOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StateTransitions)
            .WithOne(x => x.ReportedOrder)
            .HasForeignKey(x => x.ReportedOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.FailureDetails)
            .WithOne(x => x.ReportedOrder)
            .HasForeignKey(x => x.ReportedOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CustomerId);
    }
}
