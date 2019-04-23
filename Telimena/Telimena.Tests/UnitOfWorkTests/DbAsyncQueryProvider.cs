using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Telimena.Tests
{
    internal class DbAsyncQueryProvider<TEntity> : IDbAsyncQueryProvider
    {
        private readonly IQueryProvider inner;

        internal DbAsyncQueryProvider(IQueryProvider inner)
        {
            this.inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new DbAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new DbAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return this.inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return this.inner.Execute<TResult>(expression);
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Execute(expression));
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Execute<TResult>(expression));
        }
    }
}