using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Returns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public sealed class ReturnLineEntityTypeConfiguration : IEntityTypeConfiguration<ReturnLine>
{
    public void Configure(EntityTypeBuilder<ReturnLine> builder)
    {
        builder.ToTable(ReturnLineTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(ReturnLineTableConsts.IdMaxLength);

        builder.Property(x => x.ReturnId)
            .HasMaxLength(ReturnLineTableConsts.ReturnIdMaxLength);

        builder.Property(x => x.OrderItemId)
            .HasMaxLength(ReturnLineTableConsts.OrderItemIdMaxLength);

        builder.Property(x => x.ProductId)
            .HasMaxLength(ReturnLineTableConsts.ProductIdMaxLength);

        builder.Property(x => x.Quantity);

        builder.MapAudited();

        builder.HasIndex(x => x.ReturnId);
        builder.HasIndex(x => x.OrderItemId);
    }
}
