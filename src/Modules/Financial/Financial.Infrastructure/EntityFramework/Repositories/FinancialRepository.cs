using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.Financial.Domain.Repositories;
using Invoria.Financial.Infrastructure.EntityFramework;

namespace Invoria.Financial.Infrastructure.EntityFramework.Repositories
{
    public class FinancialRepository<TEntity> : EFCoreRepository<TEntity, FinancialDbContext>, IFinancialRepository<TEntity>
        where TEntity : class, IBaseEntity
    {
        public FinancialRepository(FinancialDbContext dbContext) : base(dbContext)
        {
        }
    }
}
