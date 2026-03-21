using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Infrastructure.EntityFramework;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Repositories
{
    public class OrderingRepository<TEntity> : EFCoreRepository<TEntity, OrderingDbContext>, IOrderingRepository<TEntity>
        where TEntity : class, IBaseEntity
    {
        public OrderingRepository(OrderingDbContext dbContext) : base(dbContext)
        {
        }
    }
}
