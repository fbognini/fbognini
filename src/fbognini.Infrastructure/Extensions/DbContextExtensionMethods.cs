using fbognini.Core.Entities;
using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Extensions
{
    public static class DbContextExtensionMethods
    {
        public static void ConfigureSqlServer(this DbContextOptionsBuilder optionsBuilder, IBaseDbContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.ConnectionString))
            {
                optionsBuilder.UseSqlServer(context.ConnectionString);
            }
        }

        public static void ApplyConfigurationsAndFilters(this ModelBuilder builder, IBaseDbContext context)
        {
            builder.ApplyConfigurationsFromAssembly(context.GetType().GetTypeInfo().Assembly);
            builder.ApplyGlobalFilters<IHaveTenant>(b => EF.Property<string>(b, nameof(IHaveTenant.Tenant)) == context.Tenant);
            builder.ApplyGlobalFilters<ISoftDelete>(s => s.Deleted == null);
        }

        public static void SetNewProperty(this IAuditableEntity entity, string name, string? value)
        {
            var newProperty = entity.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (newProperty != null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    newProperty.SetValue(entity, null);
                }
                else
                {
                    newProperty.SetValue(entity, TypeDescriptor.GetConverter(newProperty.PropertyType).ConvertFromString(value));
                }

            }
        }


        private static void FillAuditablePropertys<TContext>(this TContext context)
            where TContext: DbContext, IBaseDbContext
        {
            foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.FillAuditablePropertysAdded(context);
                        break;
                    case EntityState.Modified:
                        entry.Entity.FillAuditablePropertysModified(context);
                        break;
                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete softDelete)
                        {
                            entry.Entity.SetNewProperty(nameof(ISoftDelete.DeletedBy), context.UserId);
                            softDelete.DeletedBy = context.UserId;
                            softDelete.Deleted = context.Timestamp;
                            entry.State = EntityState.Modified;
                        }

                        break;
                }
            }
        }


        public static void FillAuditablePropertysAdded(this IAuditableEntity entity, IBaseDbContext context)
        {
            entity.SetNewProperty(nameof(IAuditableEntity.CreatedBy), context.UserId);
            entity.SetNewProperty(nameof(IAuditableEntity.LastUpdatedBy), context.UserId);
            entity.CreatedBy = context.UserId;
            entity.Created = context.Timestamp;
            entity.LastUpdatedBy = context.UserId;
            entity.LastUpdated = context.Timestamp;
        }

        public static void FillAuditablePropertysModified(this IAuditableEntity entity, IBaseDbContext context)
        {
            entity.SetNewProperty(nameof(IAuditableEntity.LastModifiedBy), context.UserId);
            entity.SetNewProperty(nameof(IAuditableEntity.LastUpdatedBy), context.UserId);
            entity.LastModifiedBy = context.UserId;
            entity.LastModified = context.Timestamp;
            entity.LastUpdatedBy = context.UserId;
            entity.LastUpdated = context.Timestamp;
        }

        public static async Task<int> AuditableSaveChangesAsync<TContext>(this TContext context, CancellationToken cancellationToken = new CancellationToken())
            where TContext : DbContext, IBaseDbContext
        {
            context.FillAuditablePropertys();

            if (context.UserId == null)
            {
                return await context.BaseSaveChangesAsync(cancellationToken);
            }

            var entrys = await context.OnBeforeSaveChanges(cancellationToken);
            var result = await context.BaseSaveChangesAsync(cancellationToken);
            await context.OnAfterSaveChanges(entrys, cancellationToken);

            return result;
        }

        private static async Task<List<AuditEntry>> OnBeforeSaveChanges<TContext>(this TContext context, CancellationToken cancellationToken)
            where TContext : DbContext, IBaseDbContext
        {
            var auditEntrys = new List<AuditEntry>();

            context.ChangeTracker.DetectChanges();
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                {
                    continue;
                }

                var propertys = entry.State == EntityState.Deleted
                    ? entry.Properties
                    : entry.Properties.Where(x => !typeof(IAuditableEntity).GetProperties().Select(x => x.Name).Any(a => a == x.Metadata.Name));

                var originalValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                var auditEntry = new AuditEntry(entry)
                {
                    UserId = context.UserId,
                    DateTime = context.Timestamp
                };
                foreach (var property in propertys)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    var propertyName = property.Metadata.Name;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    if (entry.State == EntityState.Added)
                    {
                        auditEntry.AuditType = AuditType.Create;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    var originalValue = originalValues[propertyName];

                    if (entry.State == EntityState.Deleted)
                    {
                        auditEntry.AuditType = AuditType.Delete;
                        auditEntry.OldValues[propertyName] = originalValue;
                        continue;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        if (property.IsModified && entry.Entity is ISoftDelete && property.OriginalValue == null && property.CurrentValue != null)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = originalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        else if (property.IsModified && originalValue?.Equals(property.CurrentValue) == false)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Update;
                            auditEntry.OldValues[propertyName] = originalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }

                        continue;
                    }
                }

                if (auditEntry.HasProperties)
                {
                    auditEntrys.Add(auditEntry);
                }
            }
            foreach (var auditEntry in auditEntrys.Where(_ => !_.HasTemporaryProperties))
            {
                context.AuditTrails.Add(auditEntry.ToAudit());
            }
            return auditEntrys.Where(_ => _.HasTemporaryProperties).ToList();
        }


        private static async Task OnAfterSaveChanges<TContext>(this TContext context, List<AuditEntry> auditEntries, CancellationToken cancellationToken)
            where TContext : DbContext, IBaseDbContext
        {
            if (auditEntries == null || auditEntries.Count == 0)
            {
                return;
            }

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

            await context.BaseSaveChangesAsync(cancellationToken);
        }
    }
}
