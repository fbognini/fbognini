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
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace fbognini.Infrastructure.Identity.Persistence
{
    public class AuditableContext<TContext, TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>, IBaseDbContext
        where TContext : DbContext
        where TUser : AuditableUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ITenantInfo currentTenant;
        private readonly ICurrentUserService currentUserService;
        private readonly string authSchema;

        public AuditableContext(
            DbContextOptions<TContext> options,
            ITenantInfo currentTenant,
            ICurrentUserService currentUserService,
            string authSchema = "auth")
            : base(options)
        {
            this.currentTenant = currentTenant;
            this.currentUserService = currentUserService;
            this.authSchema = authSchema;
        }

        public DbSet<Audit> AuditTrails { get; set; }
        public string Tenant => currentTenant.Name;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();

            if (!string.IsNullOrWhiteSpace(currentTenant?.ConnectionString))
            {
                optionsBuilder.UseSqlServer(currentTenant.ConnectionString);
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
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.CreatedBy));
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.LastUpdatedBy));
                        entry.Entity.CreatedBy = currentUserService.UserId;
                        entry.Entity.Created = DateTime.Now;
                        entry.Entity.LastUpdatedBy = currentUserService.UserId;
                        entry.Entity.LastUpdated = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.LastModifiedBy));
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.LastUpdatedBy));
                        entry.Entity.LastModifiedBy = currentUserService.UserId;
                        entry.Entity.LastModified = DateTime.Now;
                        entry.Entity.LastUpdatedBy = currentUserService.UserId;
                        entry.Entity.LastUpdated = DateTime.Now;
                        break;
                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete softDelete)
                        {
                            SetNewProperty(entry.Entity, nameof(ISoftDelete.DeletedBy));
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

            void SetNewProperty(IAuditableEntity entity, string name)
            {
                var newProperty = entity.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (newProperty != null)
                {
                    newProperty.SetValue(entity, TypeDescriptor.GetConverter(newProperty.PropertyType).ConvertFromString(currentUserService.UserId));
                }
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

    }
}
