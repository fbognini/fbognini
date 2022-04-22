using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data.Common;
using Snickler.EFCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using fbognini.Core.Interfaces;
using fbognini.Application.DbContexts;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System;
using EFCore.BulkExtensions;
using System.Linq.Expressions;
using fbognini.Application.Entities;
using fbognini.Core.Entities;

namespace fbognini.Infrastructure.Persistence
{
    public class BaseDbContext<T> : DbContext, IBaseDbContext
        where T : DbContext
    {
        public readonly ICurrentUserService currentUserService;

        public BaseDbContext(
            DbContextOptions<T> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            this.currentUserService = currentUserService;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = currentUserService.UserId;
                        entry.Entity.Created = DateTime.Now;
                        entry.Entity.LastUpdatedBy = currentUserService.UserId;
                        entry.Entity.LastUpdated = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = currentUserService.UserId;
                        entry.Entity.LastModified = DateTime.Now;
                        entry.Entity.LastUpdatedBy = currentUserService.UserId;
                        entry.Entity.LastUpdated = DateTime.Now;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        #region Utils

        public void DetachAllEntities()
        {
            var changedEntriesCopy = this.ChangeTracker.Entries()
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        public DbCommand LoadStoredProcedure(string storedProcName, bool prependDefaultSchema = true, short commandTimeout = 30)
        {
            return this.LoadStoredProc(storedProcName, prependDefaultSchema, commandTimeout);
        }

        public IDbContextTransaction BeginTransaction()
        {
            return Database.BeginTransaction();
        }

        public IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            return Database.UseTransaction(transaction);
        }

        #endregion

        #region MassiveInsert

        public void MassiveInsert<T1>(IList<T1> entities, BulkConfig bulkConfig = null, Action<decimal> progress = null) where T1 : class
        {
            if (entities.First() is AuditableEntity)
            {
                foreach (var entry in entities)
                {
                    (entry as AuditableEntity).CreatedBy = currentUserService.UserId;
                    (entry as AuditableEntity).Created = DateTime.Now;
                    (entry as AuditableEntity).LastUpdatedBy = currentUserService.UserId;
                    (entry as AuditableEntity).LastUpdated = DateTime.Now;

                }
            }
            this.BulkInsert(entities, bulkConfig, progress);
        }

        public void MassiveInsert<T1>(IList<T1> entities, Action<BulkConfig> bulkAction, Action<decimal> progress = null) where T1 : class
        {
            if (entities.First() is AuditableEntity)
            {
                foreach (var entry in entities)
                {
                    (entry as AuditableEntity).CreatedBy = currentUserService.UserId;
                    (entry as AuditableEntity).Created = DateTime.Now;
                    (entry as AuditableEntity).LastUpdatedBy = currentUserService.UserId;
                    (entry as AuditableEntity).LastUpdated = DateTime.Now;

                }
            }
            this.BulkInsert(entities, bulkAction, progress);
        }

        #endregion

        #region MassiveDelete

        public void MassiveDelete<T1>(IList<T1> entities, Action<BulkConfig> bulkAction, Action<decimal> progress = null) where T1 : class
        {
            this.BulkDelete(entities, bulkAction, progress);
        }

        public void MassiveDelete<T1>(IList<T1> entities, BulkConfig bulkConfig = null, Action<decimal> progress = null) where T1 : class
        {
            this.BulkDelete(entities, bulkConfig, progress);
        }

        #endregion

        #region MassiveUpdate

        public void MassiveUpdate<T1>(IList<T1> entities, Action<BulkConfig> bulkAction, Action<decimal> progress = null) where T1 : class
        {
            if (entities.First() is AuditableEntity)
            {
                foreach (var entry in entities)
                {
                    (entry as AuditableEntity).LastModifiedBy = currentUserService.UserId;
                    (entry as AuditableEntity).LastModified = DateTime.Now;
                    (entry as AuditableEntity).LastUpdatedBy = currentUserService.UserId;
                    (entry as AuditableEntity).LastUpdated = DateTime.Now;

                }
            }
            this.BulkUpdate(entities, bulkAction, progress);
        }

        public void MassiveUpdate<T1>(IList<T1> entities, BulkConfig bulkConfig = null, Action<decimal> progress = null) where T1 : class
        {
            if (entities.First() is AuditableEntity)
            {
                foreach (var entry in entities)
                {
                    (entry as AuditableEntity).LastModifiedBy = currentUserService.UserId;
                    (entry as AuditableEntity).LastModified = DateTime.Now;
                    (entry as AuditableEntity).LastUpdatedBy = currentUserService.UserId;
                    (entry as AuditableEntity).LastUpdated = DateTime.Now;
                }
            }
            this.BulkUpdate(entities, bulkConfig, progress);
        }
        #endregion

        #region BatchDelete

        public int BatchDelete(IQueryable query)
        {
            return query.BatchDelete();
        }

        public Task<int> BatchDeleteAsync(IQueryable query, CancellationToken cancellationToken = default)
        {
            return query.BatchDeleteAsync(cancellationToken);
        }

        #endregion

        #region BatchUpdate

        public int BatchUpdate<T1>(IQueryable<T1> query, Expression<Func<T1, T1>> updateExpression) where T1 : class
        {
            if (updateExpression.Body.Type.IsSubclassOf(typeof(AuditableEntity)) && updateExpression.Body is MemberInitExpression memberInitExpression)
            {
                var bindingExpressions = new List<MemberAssignment>
                {
                    Expression.Bind(typeof(T1).GetProperty("LastModifiedBy"), Expression.Constant(currentUserService.UserId)),
                    Expression.Bind(typeof(T1).GetProperty("LastModified"), Expression.Constant(DateTime.Now)),
                    Expression.Bind(typeof(T1).GetProperty("LastUpdatedBy"), Expression.Constant(currentUserService.UserId)),
                    Expression.Bind(typeof(T1).GetProperty("LastUpdated"), Expression.Constant(DateTime.Now)),
                };

                bindingExpressions.AddRange(memberInitExpression.Bindings.Select(x => x as MemberAssignment));

                var body = Expression.MemberInit(Expression.New(typeof(T1)), bindingExpressions);
                var lambda = Expression.Lambda<Func<T1, T1>>(body, Expression.Parameter(typeof(T1), updateExpression.Parameters[0].Name));

                return query.BatchUpdate(lambda);
            }

            return query.BatchUpdate(updateExpression);
        }

        public Task<int> BatchUpdateAsync(IQueryable query, object updateValues, List<string> updateColumns = null, CancellationToken cancellationToken = default)
        {
            if (updateValues is AuditableEntity)
            {
                (updateValues as AuditableEntity).LastModifiedBy = currentUserService.UserId;
                (updateValues as AuditableEntity).LastModified = DateTime.Now;
                (updateValues as AuditableEntity).LastUpdatedBy = currentUserService.UserId;
                (updateValues as AuditableEntity).LastUpdated = DateTime.Now;
            }
            return query.BatchUpdateAsync(updateValues, updateColumns, cancellationToken);
        }

        public Task<int> BatchUpdateAsync<T1>(IQueryable<T1> query, Expression<Func<T1, T1>> updateExpression, CancellationToken cancellationToken = default) where T1 : class
        {
            if (updateExpression.Body.Type.IsSubclassOf(typeof(AuditableEntity)) && updateExpression.Body is MemberInitExpression memberInitExpression)
            {
                var bindingExpressions = new List<MemberAssignment>
                {
                    Expression.Bind(typeof(T1).GetProperty("LastModifiedBy"), Expression.Constant(currentUserService.UserId)),
                    Expression.Bind(typeof(T1).GetProperty("LastModified"), Expression.Constant(DateTime.Now)),
                    Expression.Bind(typeof(T1).GetProperty("LastUpdatedBy"), Expression.Constant(currentUserService.UserId)),
                    Expression.Bind(typeof(T1).GetProperty("LastUpdated"), Expression.Constant(DateTime.Now)),
                };

                bindingExpressions.AddRange(memberInitExpression.Bindings.Select(x => x as MemberAssignment));

                var body = Expression.MemberInit(Expression.New(typeof(T1)), bindingExpressions);
                var lambda = Expression.Lambda<Func<T1, T1>>(body, Expression.Parameter(typeof(T1), updateExpression.Parameters[0].Name));

                return query.BatchUpdateAsync(lambda, cancellationToken: cancellationToken);
            }

            return query.BatchUpdateAsync(updateExpression, cancellationToken: cancellationToken);
        }

        #endregion
    }
}
