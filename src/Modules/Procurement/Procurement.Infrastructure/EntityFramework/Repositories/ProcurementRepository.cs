using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Infrastructure.EntityFramework;

namespace Invoria.Procurement.Infrastructure.EntityFramework.Repositories
{
    public class ProcurementRepository<TEntity> : EFCoreRepository<TEntity, ProcurementDbContext>, IProcurementRepository<TEntity>
        where TEntity : class, IBaseEntity
    {
        public ProcurementRepository(ProcurementDbContext dbContext) : base(dbContext)
        {
        }
    }
}
