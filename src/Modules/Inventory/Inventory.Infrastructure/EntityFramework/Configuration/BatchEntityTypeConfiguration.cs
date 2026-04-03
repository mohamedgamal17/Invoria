using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Configuration
{
    public class BatchEntityTypeConfiguration : IEntityTypeConfiguration<Batch>
    {
        public void Configure(EntityTypeBuilder<Batch> builder)
        {
            builder.ToTable(BatchTableConsts.TableName);

            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(BatchTableConsts.IdMaxLength);

            builder.Property(x => x.ProductId)
                .HasMaxLength(BatchTableConsts.ProductIdMaxLength);

            builder.Property(x => x.Quantity);
            builder.Property(x => x.ReservedQuantity);
            builder.Property(x => x.PurchasePrice)
                .HasPrecision(18, 2);
            builder.Property(x => x.State);

            builder.HasMany<BatchAllocation>().WithOne()
                .HasForeignKey(x => x.BatchId);
            builder.MapAudited();
        }
    }
}
