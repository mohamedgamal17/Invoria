using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.CustomerManagement.Domain.Customers;
using Invoria.CustomerManagement.Infrastructure.EntityFramework;

namespace Invoria.CustomerManagement.Infrastructure.EntityFramework.Repositories
{
    public class CustomerRepository<TEntity> : EFCoreRepository<TEntity, CustomerManagementDbContext>, ICustomerRepository<TEntity>
        where TEntity : class, IBaseEntity
    {
        public CustomerRepository(CustomerManagementDbContext dbContext) : base(dbContext)
        {
        }
    }
}

