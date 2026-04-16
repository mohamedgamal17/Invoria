using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.Procurement.Domain.Repositories
{
    public interface IProcurementRepository<T> : IRepository<T>
        where T : IBaseEntity
    {
    }
}
