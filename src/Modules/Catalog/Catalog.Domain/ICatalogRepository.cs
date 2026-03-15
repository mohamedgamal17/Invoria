using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.Modules.Catalog.Domain
{
    public interface ICatalogRepository<T> : IRepository<T> 
           where T : IBaseEntity
    {
    }
}
