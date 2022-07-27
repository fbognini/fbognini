using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data.Common;
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
using fbognini.Core.Entities;
using Snickler.EFCore;

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
    }
}
