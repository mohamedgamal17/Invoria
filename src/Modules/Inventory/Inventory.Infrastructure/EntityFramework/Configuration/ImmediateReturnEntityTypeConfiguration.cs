using Invoria.Inventory.Domain.Returns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public sealed class ImmediateReturnEntityTypeConfiguration : IEntityTypeConfiguration<ImmediateReturn>
{
    public void Configure(EntityTypeBuilder<ImmediateReturn> builder)
    {
        builder.Property(x => x.AllocationId)
            .HasMaxLength(ImmediateReturnTableConsts.AllocationIdMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.AllocationId);
    }
}
