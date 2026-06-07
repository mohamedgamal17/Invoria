using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Returns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public sealed class ReturnEntityTypeConfiguration : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable(ReturnTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReturnTableConsts.IdMaxLength);

        builder.Property(x => x.OrderId)
            .HasMaxLength(ReturnTableConsts.OrderIdMaxLength);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.MapAudited();

        builder.HasDiscriminator(x => x.Type)
            .HasValue<ImmediateReturn>(ReturnType.Immediate);

        builder.HasIndex(x => x.OrderId);

        builder
            .Navigation(r => r.ReturnLines)
            .HasField("_returnLines")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany<ReturnLine>(x => x.ReturnLines)
            .WithOne()
            .HasForeignKey(x => x.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
