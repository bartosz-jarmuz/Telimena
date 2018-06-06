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

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this.DbContext.Set<TEntity>().FirstOrDefaultAsync();
            }
            else
            {
                return this.DbContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
            }
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate=null)
        {
            if (predicate == null)
            {
                return this.DbContext.Set<TEntity>().SingleOrDefaultAsync();
            }
            else
            {
                return this.DbContext.Set<TEntity>().SingleOrDefaultAsync(predicate);
            }
        }

        public virtual void Add(TEntity entity)
        {
            this.DbContext.Set<TEntity>().Add(entity);
        }

        public virtual Task<int> CountAsync()
        {
            return this.DbContext.Set<TEntity>().CountAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null,
                                            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                            string includeProperties = "")
        {
            IQueryable<TEntity> query = this.DbContext.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }

        }

        public virtual Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.DbContext.Set<TEntity>().Where(predicate).ToListAsync();  
        }

        public virtual TEntity GetById(int id)
        {
            return this.DbContext.Set<TEntity>().Find(id);

        }

        public virtual void Remove(TEntity entity)
        {
            this.DbContext.Set<TEntity>().Remove(entity);

        }
    }
}

