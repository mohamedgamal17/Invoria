
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.Catalog.Domain;

namespace Invoria.Catalog.Infrastructure.EntityFramework.Repositories
{
    public class CatalogRepository<TEntity> : EFCoreRepository<TEntity, CatalogDbContext>, ICatalogRepository<TEntity>
          where TEntity : class, IBaseEntity
    {
        public CatalogRepository(CatalogDbContext dbContext) : base(dbContext)
        {
        }
    }
}
