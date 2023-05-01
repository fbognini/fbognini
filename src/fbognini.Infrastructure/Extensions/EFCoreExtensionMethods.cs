using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fbognini.Infrastructure.Extensions
{
    public static class EFCoreExtensionMethods
    {
        public static IEnumerable<string> FindPrimaryKeyNames<T>(this DbSet<T> dbSet)
            where T: class
        {
            var model = dbSet.GetService<IDbContextServices>().Model;
            var keys = model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(x => x.Name);
            return keys;
        }

        public static IEnumerable<string> FindPrimaryKeyNames<T>(this DbContext context, T entity)
        {
            return from p in context.FindPrimaryKeyProperties(entity)
                   select p.Name;
        }

        public static IEnumerable<object> FindPrimaryKeyValues<T>(this DbContext context, T entity)
        {
            return from p in context.FindPrimaryKeyProperties(entity)
                   select entity.GetPropertyValue(p.Name);
        }

        static IReadOnlyList<IProperty> FindPrimaryKeyProperties<T>(this DbContext context, T entity)
        {
            return context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties;
        }

        static object GetPropertyValue<T>(this T entity, string name)
        {
            return entity.GetType().GetProperty(name).GetValue(entity, null);
        }

        //Get DB Table Name
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            // We need dbcontext to access the models
            var models = context.Model;

            // Get all the entity types information
            var entityTypes = models.GetEntityTypes();

            // T is Name of class
            var entityTypeOfT = entityTypes.First(t => t.ClrType == typeof(T));

            var tableNameAnnotation = entityTypeOfT.GetAnnotation("Relational:TableName");
            var TableName = tableNameAnnotation.Value.ToString();
            return TableName;
        }

        public static DbContext GetDbContext<T>(this DbSet<T> dbSet) where T : class
        {
            var infrastructure = dbSet as IInfrastructure<IServiceProvider>;
            var serviceProvider = infrastructure.Instance;
            var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext))
                                       as ICurrentDbContext;
            return currentDbContext.Context;
        }

        public static string GetTableName<T>(this DbSet<T> set) where T : class
        {
            return set.GetDbContext().GetTableName<T>();
        }


    }
}
