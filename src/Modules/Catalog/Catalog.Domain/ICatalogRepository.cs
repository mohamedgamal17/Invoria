using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.Catalog.Domain
{
    public interface ICatalogRepository<T> : IRepository<T> 
           where T : IBaseEntity
    {
    }
}
