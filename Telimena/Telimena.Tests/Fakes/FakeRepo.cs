namespace Telimena.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using WebApp.Infrastructure.Repository;

    public class FakeRepo<T> : IRepository<T> where T: class, new()
    {
        public List<T> Data { get; set; } = new List<T>();
        public void Add(T entity)
        {
            this.Data.Add(entity);
        }

        public Task<int> CountAsync()
        {
            return Task.FromResult(this.Data.Count);
        }

        public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return Task.FromResult(this.Data.AsQueryable().Where(predicate).ToList());

        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return Task.FromResult(this.Data.AsQueryable().FirstOrDefault(predicate));
        }

        public T Get(int id)
        {
            var idProperty = typeof(T).GetProperty("Id");
            return this.Data.FirstOrDefault(x => (int)idProperty.GetValue(x) == (int)id);
        }

        public Task<List<T>> GetAllAsync()
        {
            return Task.FromResult(this.Data.ToList()); 
        }

        public void Remove(T entity)
        {
            this.Data.Remove(entity);
        }
    }
}