using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.Reporting.Domain.Repositories;
using Invoria.Reporting.Infrastructure.EntityFramework;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Repositories
{
    public class ReportingRepository<TEntity> : EFCoreRepository<TEntity, ReportingDbContext>, IReportingRepository<TEntity>
        where TEntity : class, IBaseEntity
    {
        public ReportingRepository(ReportingDbContext dbContext) : base(dbContext)
        {
        }
    }
}
