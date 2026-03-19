using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.CustomerManagement.Domain.Customers
{
    public interface ICustomerRepository<T> : IRepository<T>
        where T : IBaseEntity
    {
    }
}
