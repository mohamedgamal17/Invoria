using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.Reporting.Domain.Repositories
{
    public interface IReportingRepository<T> : IRepository<T>
        where T : IBaseEntity
    {
    }
}
