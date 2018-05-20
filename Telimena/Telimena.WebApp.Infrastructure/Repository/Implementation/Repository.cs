namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        protected DbContext DbContext;

        public Repository(DbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public virtual void Add(TEntity entity)
        {
            this.DbContext.Set<TEntity>().Add(entity);
        }

        public virtual void CountAsync()
        {
            this.DbContext.Set<TEntity>().CountAsync();
        }

        public virtual Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.DbContext.Set<TEntity>().Where(predicate).ToListAsync();  
        }

        public virtual TEntity Get(int id)
        {
            return this.DbContext.Set<TEntity>().Find(id);

        }

        public virtual void Remove(TEntity entity)
        {
            this.DbContext.Set<TEntity>().Remove(entity);

        }
    }
}
