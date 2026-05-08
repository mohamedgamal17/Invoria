using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Catalog.Infrastructure.EntityFramework.Configuration
{
    public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {

            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(ProductTableConsts.IdMaxLength);

            builder.Property(x => x.Name).HasMaxLength(ProductTableConsts.NameMaxLength);

            builder.MapAudited();
        }
    }
}
