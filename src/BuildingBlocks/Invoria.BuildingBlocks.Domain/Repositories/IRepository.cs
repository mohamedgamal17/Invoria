using Invoria.BuildingBlocks.Domain.Entities;
using System.Linq.Expressions;

namespace Invoria.BuildingBlocks.Domain.Repositories
{
    public interface IRepository<TEntity> where TEntity : IBaseEntity
    {
        Task<TEntity> Add(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity> Update(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity> Delete(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity> Single(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        Task<TEntity?> SingleOrDefault(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        IQueryable<TEntity> AsQuerable();
    }
}
