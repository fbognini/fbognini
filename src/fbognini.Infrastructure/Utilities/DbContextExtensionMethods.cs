using fbognini.Application.DbContexts;
using fbognini.Application.Entities;
using fbognini.Core.Entities;
using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Models;
using fbognini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Utilities
{
    public static class DbContextExtensionMethods
    {
        public static void OnCustomConfiguring(this DbContextOptionsBuilder optionsBuilder, IBaseDbContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.ConnectionString))
            {
                optionsBuilder.UseSqlServer(context.ConnectionString);
            }
        }

        public static void OnCustomModelCreating(this ModelBuilder builder, IBaseDbContext context)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.ApplyGlobalFilters<IHaveTenant>(b => EF.Property<string>(b, nameof(IHaveTenant.Tenant)) == context.Tenant);
            builder.ApplyGlobalFilters<ISoftDelete>(s => s.Deleted == null);
        }

        public static async Task<int> AuditableSaveChangesAsync<TContext>(this TContext context, Func<Task<int>> baseSaveChanges, CancellationToken cancellationToken = new CancellationToken())
            where TContext : DbContext, IBaseDbContext
        {
            foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.CreatedBy));
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.LastUpdatedBy));
                        entry.Entity.CreatedBy = context.UserId;
                        entry.Entity.Created = DateTime.Now;
                        entry.Entity.LastUpdatedBy = context.UserId;
                        entry.Entity.LastUpdated = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.LastModifiedBy));
                        SetNewProperty(entry.Entity, nameof(IAuditableEntity.LastUpdatedBy));
                        entry.Entity.LastModifiedBy = context.UserId;
                        entry.Entity.LastModified = DateTime.Now;
                        entry.Entity.LastUpdatedBy = context.UserId;
                        entry.Entity.LastUpdated = DateTime.Now;
                        break;
                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete softDelete)
                        {
                            SetNewProperty(entry.Entity, nameof(ISoftDelete.DeletedBy));
                            softDelete.DeletedBy = context.UserId;
                            softDelete.Deleted = DateTime.Now;
                            entry.State = EntityState.Modified;
                        }

                        break;
                }
            }

            if (context.UserId == null)
            {
                return await baseSaveChanges();
            }
            else
            {
                var auditEntries = OnBeforeSaveChanges(context.UserId);
                var result = await baseSaveChanges();
                await OnAfterSaveChanges(auditEntries, cancellationToken);

                return result;
            }

            void SetNewProperty(IAuditableEntity entity, string name)
            {
                var newProperty = entity.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (newProperty != null)
                {
                    newProperty.SetValue(entity, TypeDescriptor.GetConverter(newProperty.PropertyType).ConvertFromString(context.UserId));
                }
            }

            List<AuditEntry> OnBeforeSaveChanges(string userId)
            {
                context.ChangeTracker.DetectChanges();
                var auditEntries = new List<AuditEntry>();
                foreach (var entry in context.ChangeTracker.Entries())
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
                    context.AuditTrails.Add(auditEntry.ToAudit());
                }
                return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
            }

            Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
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
                    context.AuditTrails.Add(auditEntry.ToAudit());
                }

                return context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
