using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration;

public class OrderDispatchProcessedEntityTypeConfiguration : IEntityTypeConfiguration<OrderDispatchProcessed>
{
    public void Configure(EntityTypeBuilder<OrderDispatchProcessed> builder)
    {
        builder.ToTable(OrderDispatchProcessedTableConsts.TableName);

        builder.MapId();

        builder.Property(x => x.Id)
            .HasMaxLength(OrderDispatchProcessedTableConsts.OrderIdMaxLength);

        builder.Property(x => x.ProcessedAt);
    }
}
