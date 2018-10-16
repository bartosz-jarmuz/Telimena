using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Telimena.Tests
{
    internal class DbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> inner;

        public DbAsyncEnumerator(IEnumerator<T> inner)
        {
            this.inner = inner;
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.inner.MoveNext());
        }

        public T Current
        {
            get { return this.inner.Current; }
        }

        object IDbAsyncEnumerator.Current
        {
            get { return this.Current; }
        }
    }
}