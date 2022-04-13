using EFCore.BulkExtensions;
using fbognini.Application.DbContexts;
using fbognini.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Snickler.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using fbognini.Application.Entities;
using fbognini.Core.Entities;
using fbognini.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using fbognini.Infrastructure.Persistence;
using Finbuckle.MultiTenant;

namespace fbognini.Infrastructure.Identity.Persistence
{
    public class AuditableContext<TContext, TUser, TRole, TKey> : MultiTenantIdentityDbContext<TUser, TRole, TKey>, IBaseDbContext
        where TContext : DbContext
        where TUser : AuditableUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ICurrentUserService currentUserService;
        private readonly string authSchema;

        public AuditableContext(
            ITenantInfo currentTenant,
            DbContextOptions<TContext> options,
            ICurrentUserService currentUserService,
            string authSchema = "auth")
            : base(currentTenant, options)
        {
            this.currentUserService = currentUserService;
            this.authSchema = authSchema;
        }

        public DbSet<Audit> AuditTrails { get; set; }
        public string Tenant => TenantInfo.Name;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();

            if (!string.IsNullOrWhiteSpace(TenantInfo?.ConnectionString))
            {
                //optionsBuilder.UseDatabase(_dbSettings.DBProvider!, TenantInfo.ConnectionString);
                optionsBuilder.UseSqlServer(TenantInfo.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.ApplyIdentityConfiguration<TUser, TRole, TKey>(authSchema);
            builder.ApplyGlobalFilters<IHaveTenant>(b => EF.Property<string>(b, nameof(Tenant)) == Tenant);
            builder.ApplyGlobalFilters<ISoftDelete>(s => s.Deleted == null);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
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
                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete softDelete)
                        {
                            softDelete.DeletedBy = currentUserService.UserId;
                            softDelete.Deleted = DateTime.Now;
                            entry.State = EntityState.Modified;
                        }

                        break;
                }
            }

            if (currentUserService.UserId == null)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var auditEntries = OnBeforeSaveChanges(currentUserService.UserId); 
                var result = await base.SaveChangesAsync(cancellationToken);
                await OnAfterSaveChanges(auditEntries, cancellationToken);

                return result;
            }
        }

        private List<AuditEntry> OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = userId
                };
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified && entry.Entity is ISoftDelete && property.OriginalValue == null && property.CurrentValue != null)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Delete;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            else if (property.IsModified && property.OriginalValue?.Equals(property.CurrentValue) == false)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                AuditTrails.Add(auditEntry.ToAudit());
            }
            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                AuditTrails.Add(auditEntry.ToAudit());
            }
            return SaveChangesAsync(cancellationToken);
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

        public int BatchUpdate(IQueryable query, object updateValues, List<string> updateColumns = null)
        {
            if (updateValues is AuditableEntity)
            {
                (updateValues as AuditableEntity).LastModifiedBy = currentUserService.UserId;
                (updateValues as AuditableEntity).LastModified = DateTime.Now;
                (updateValues as AuditableEntity).LastUpdatedBy = currentUserService.UserId;
                (updateValues as AuditableEntity).LastUpdated = DateTime.Now;
            }

            return query.BatchUpdate(updateValues, updateColumns);
        }

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
