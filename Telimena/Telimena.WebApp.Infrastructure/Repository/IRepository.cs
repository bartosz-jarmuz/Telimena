namespace Telimena.WebApp.Infrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IRepository<TEntity> where TEntity : class, new()
    {
        TEntity Get(int id);
        Task<List<TEntity>> GetAllAsync();

        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        void Add(TEntity entity);

        void Remove(TEntity entity);
        Task<int> CountAsync();
    }
}