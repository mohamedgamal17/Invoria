using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Allocations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public sealed class AllocationEntityTypeConfiguration : IEntityTypeConfiguration<Allocation>
{
    public void Configure(EntityTypeBuilder<Allocation> builder)
    {
        builder.ToTable(AllocationTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(AllocationTableConsts.IdMaxLength);

        builder.Property(x => x.OrderId)
            .HasMaxLength(AllocationTableConsts.OrderIdMaxLength);

        builder.Property(x => x.Status);

        builder.MapAudited();

        builder
            .Navigation(a => a.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasMany<AllocationLine>(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.AllocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
