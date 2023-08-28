using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Repositorys
{
    public sealed class RepositoryTransaction : IDisposable, IAsyncDisposable
    {
        private readonly IDbContextTransaction transaction;

        internal RepositoryTransaction(IDbContextTransaction transaction)
        {
            this.transaction = transaction;
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Rollback()
        {
            transaction.Rollback();
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        public void Dispose()
        {
            transaction.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await transaction.DisposeAsync();
        }
    }
}
