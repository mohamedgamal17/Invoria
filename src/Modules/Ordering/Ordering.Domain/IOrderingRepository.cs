using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.Ordering.Domain
{
    public interface IOrderingRepository<T> : IRepository<T>
        where T : IBaseEntity
    {
    }
}
