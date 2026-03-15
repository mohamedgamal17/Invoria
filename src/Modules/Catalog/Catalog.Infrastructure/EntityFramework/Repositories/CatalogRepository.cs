
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.Modules.Catalog.Domain;

namespace Invoria.Modules.Catalog.Infrastructure.EntityFramework.Repositories
{
    public class CatalogRepository<TEntity> : EFCoreRepository<TEntity, CatalogDbContext>, ICatalogRepository<TEntity>
          where TEntity : class, IBaseEntity
    {
        public CatalogRepository(CatalogDbContext dbContext) : base(dbContext)
        {
        }
    }
}
