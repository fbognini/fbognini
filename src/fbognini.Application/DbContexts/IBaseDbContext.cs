using EFCore.BulkExtensions;
using fbognini.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.DbContexts
{
    public interface IBaseDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DbCommand LoadStoredProcedure(string storedProcName, bool prependDefaultSchema = true, short commandTimeout = 30);
        IDbContextTransaction BeginTransaction();
        IDbContextTransaction UseTransaction(DbTransaction transaction);

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        void DetachAllEntities();

        void MassiveInsert<T>(IList<T> entities, BulkConfig bulkConfig = null, Action<decimal> progress = null) where T : class;
        void MassiveInsert<T>(IList<T> entities, Action<BulkConfig> bulkAction, Action<decimal> progress = null) where T : class;

        void MassiveDelete<T>(IList<T> entities, Action<BulkConfig> bulkAction, Action<decimal> progress = null) where T : class;
        void MassiveDelete<T>(IList<T> entities, BulkConfig bulkConfig = null, Action<decimal> progress = null) where T : class;

        void MassiveUpdate<T>(IList<T> entities, Action<BulkConfig> bulkAction, Action<decimal> progress = null) where T : class;
        void MassiveUpdate<T>(IList<T> entities, BulkConfig bulkConfig = null, Action<decimal> progress = null) where T : class;

        int BatchDelete(IQueryable query);
        Task<int> BatchDeleteAsync(IQueryable query, CancellationToken cancellationToken = default);
        
        int BatchUpdate<T1>(IQueryable<T1> query, Expression<Func<T1, T1>> updateExpression) where T1 : class;
        Task<int> BatchUpdateAsync(IQueryable query, object updateValues, List<string> updateColumns = null, CancellationToken cancellationToken = default);
        Task<int> BatchUpdateAsync<T1>(IQueryable<T1> query, Expression<Func<T1, T1>> updateExpression, CancellationToken cancellationToken = default) where T1 : class;
    }
}
