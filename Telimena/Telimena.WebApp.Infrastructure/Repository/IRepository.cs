namespace Telimena.WebApp.Infrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IRepository<TEntity> where TEntity : class, new()
    {
        TEntity GetById(int id);
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null,
                                     Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                     string includeProperties = "");

        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null);

        void Add(TEntity entity);

        void Remove(TEntity entity);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null);
        TEntity Single(Expression<Func<TEntity, bool>> predicate = null);
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate = null);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate = null);
    }
}