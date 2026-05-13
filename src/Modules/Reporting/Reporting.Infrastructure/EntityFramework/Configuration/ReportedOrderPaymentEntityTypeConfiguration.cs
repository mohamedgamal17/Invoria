using Invoria.Reporting.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Configuration;

public sealed class ReportedOrderPaymentEntityTypeConfiguration : IEntityTypeConfiguration<ReportedOrderPayment>
{
    public void Configure(EntityTypeBuilder<ReportedOrderPayment> builder)
    {
        builder.ToTable(ReportedOrderPaymentTableConsts.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(ReportedOrderPaymentTableConsts.IdMaxLength);

        builder.Property(x => x.ReportedOrderId)
            .HasMaxLength(ReportedOrderPaymentTableConsts.ReportedOrderIdMaxLength)
            .IsRequired();

        builder.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaymentMethod).IsRequired();
        builder.Property(x => x.PaidAt).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.LastModifiedAt);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(256);

        builder.HasIndex(x => x.ReportedOrderId);
    }
}
