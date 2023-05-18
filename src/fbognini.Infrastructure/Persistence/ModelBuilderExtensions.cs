using fbognini.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Infrastructure.Persistence
{
    public static class ModelBuilderExtensions
    {

        public static void ApplyIdentityConfiguration<TUser, TRole>(this ModelBuilder modelBuilder, string authSchema)
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            modelBuilder.ApplyIdentityConfiguration<TUser, TRole, string>(authSchema);
        }

        public static void ApplyIdentityConfiguration<TUser, TRole, TKey>(this ModelBuilder modelBuilder, string authSchema)
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TKey: IEquatable<TKey>
        {
            modelBuilder.Entity<TUser>().ToTable("Users", authSchema);
            modelBuilder.Entity<TRole>(entity =>
            {
                entity.ToTable("Roles", authSchema);
                if (entity is IHaveTenant)
                {
                    entity.Metadata.RemoveIndex(new[] { entity.Property(r => r.NormalizedName).Metadata });
                    entity.HasIndex(r => new { r.NormalizedName, (r as IHaveTenant).Tenant }).HasDatabaseName("RoleNameIndex").IsUnique();
                }
            });

            modelBuilder.Entity<IdentityRoleClaim<TKey>>().ToTable("RoleClaims", authSchema);
            modelBuilder.Entity<IdentityUserRole<TKey>>().ToTable("UserRoles", authSchema);
            modelBuilder.Entity<IdentityUserClaim<TKey>>().ToTable("UserClaims", authSchema);
            modelBuilder.Entity<IdentityUserLogin<TKey>>().ToTable("UserLogins", authSchema);
            modelBuilder.Entity<IdentityUserToken<TKey>>().ToTable("UserTokens", authSchema);
        }


        public static void ApplyGlobalFilters<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> expression)
        {
            var entities = modelBuilder.Model
                .GetEntityTypes()
                .Where(e => e.ClrType.GetInterface(typeof(TInterface).Name) != null)
                .Select(e => e.ClrType);
            foreach (var entity in entities)
            {
                var newParam = Expression.Parameter(entity);
                var newbody = ReplacingExpressionVisitor.Replace(expression.Parameters.Single(), newParam, expression.Body);
                modelBuilder.Entity(entity).HasQueryFilter(Expression.Lambda(newbody, newParam));
            }
        }
    }
}
