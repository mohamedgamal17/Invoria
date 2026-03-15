using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace Invoria.BuildingBlocks.EntityFramework.Repositories
{
    public abstract class EFCoreRepository<TEntity, TContext> : IRepository<TEntity>
         where TEntity : class, IBaseEntity
         where TContext : DbContext
    {
        protected TContext DbContext { get; }

        public EFCoreRepository(TContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<TEntity> Add(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbContext.AddAsync(entity, cancellationToken);

            await DbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task<TEntity> Update(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Update(entity);

            await DbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task<TEntity> Delete(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Remove(entity);

            await DbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public Task<TEntity> Single(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return DbContext.Set<TEntity>().SingleAsync(expression, cancellationToken);
        }

        public Task<TEntity?> SingleOrDefault(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return DbContext.Set<TEntity>().SingleOrDefaultAsync(expression, cancellationToken);
        }

        public IQueryable<TEntity> AsQuerable()
        {
            return DbContext.Set<TEntity>().AsQueryable();
        }
    }
}
