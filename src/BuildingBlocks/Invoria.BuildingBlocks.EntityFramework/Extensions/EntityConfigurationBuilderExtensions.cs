
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Generators;
using Invoria.BuildingBlocks.EntityFramework.Primitives;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.BuildingBlocks.EntityFramework.Extensions
{
    public static class EntityConfigurationBuilderExtensions
    {
        public static void MapAudited<T>(this EntityTypeBuilder<T> builder) 
            where  T : class , IAuditedEntity 
           
        {
            builder.Property(x => x.CreatedBy).HasMaxLength(256);
            builder.Property(x => x.LastModifiedBy).IsRequired(false).HasMaxLength(256);
        }

        public static void MapId<T>(this EntityTypeBuilder<T>  builder)
            where T : class , IEntity
        {
            builder.Property(x => x.Id);

            builder.Property(x => x.Id).HasMaxLength(256).HasValueGenerator<UlidStringValueGenerator>();
        }
    }
}
