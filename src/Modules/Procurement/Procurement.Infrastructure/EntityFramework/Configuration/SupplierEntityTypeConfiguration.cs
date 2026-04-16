using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Configuration;

public sealed class SupplierEntityTypeConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable(SupplierTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(SupplierTableConsts.IdMaxLength);

        builder.Property(x => x.SupplierCode)
            .HasMaxLength(SupplierTableConsts.SupplierCodeMaxLength);

        builder.Property(x => x.Name)
            .HasMaxLength(SupplierTableConsts.NameMaxLength);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(SupplierTableConsts.ContactEmailMaxLength);

        builder.Property(x => x.Phone)
            .HasMaxLength(SupplierTableConsts.PhoneMaxLength);

        builder.MapAudited();

        builder.HasIndex(x => x.SupplierCode)
            .IsUnique();
    }
}
