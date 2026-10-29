using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.Financial.Domain.Repositories
{
    public interface IFinancialRepository<T> : IRepository<T>
        where T : IBaseEntity
    {
    }
}
