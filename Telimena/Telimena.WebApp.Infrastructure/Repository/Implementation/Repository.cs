using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public Repository(DbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        protected DbContext DbContext;

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this.DbContext.Set<TEntity>().FirstOrDefaultAsync();
            }

            return this.DbContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }



        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this.DbContext.Set<TEntity>().SingleOrDefaultAsync();
            }

            return this.DbContext.Set<TEntity>().SingleOrDefaultAsync(predicate);
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this.DbContext.Set<TEntity>().SingleOrDefault();
            }

            return this.DbContext.Set<TEntity>().SingleOrDefault(predicate);
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this.DbContext.Set<TEntity>().FirstOrDefault();
            }

            return this.DbContext.Set<TEntity>().FirstOrDefault(predicate);
        }

        public TEntity Single(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this.DbContext.Set<TEntity>().Single();
            }

            return this.DbContext.Set<TEntity>().Single(predicate);
        }

        public virtual void Add(TEntity entity)
        {
            this.DbContext.Set<TEntity>().Add(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null
            , Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = PrepareQuery(this.DbContext, filter, includeProperties);

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync().ConfigureAwait(false);
            }

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null
            , string includeProperties = "")
        {
            IQueryable<TEntity> query = PrepareQuery(this.DbContext, filter, includeProperties);
            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }

            return query.ToList();
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

        public virtual Task<int> CountAsync()
        {
            return this.DbContext.Set<TEntity>().CountAsync();
        }

        internal static IQueryable<TEntity> PrepareQuery(DbContext dbContext, Expression<Func<TEntity, bool>> filter, string includeProperties)
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (string includeProperty in includeProperties.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return query;
        }
    }
}