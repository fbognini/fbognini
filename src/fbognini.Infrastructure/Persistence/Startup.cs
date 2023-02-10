using fbognini.Application.Persistence;
using fbognini.Infrastructure.Common;
using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Multitenancy;
using fbognini.Infrastructure.Persistence.ConnectionString;
using fbognini.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fbognini.Infrastructure.Persistence
{
    public static class Startup
    {
        private static IServiceCollection AddBasePersistence<T, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext
            where TTenant : Tenant, new()
        {
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();

            return services
                .Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)))
                .AddDbContext<T>(m => {
                    m.UseSqlServer(databaseSettings.ConnectionString);
                    if (databaseSettings.UseNoTracking)
                    {
                        m.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    }
                })
                .AddTransient<ApplicationDatabaseInitializer<T>>()
                .AddServices(typeof(ICustomSeeder<T>), ServiceLifetime.Transient)
                .AddTransient<ApplicationSeederRunner<T>>()
                .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
                .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()
                ;
        }

        public static IServiceCollection AddPersistence<T, TTenantContext, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext
            where TTenantContext : TenantDbContext<TTenant>
            where TTenant : Tenant, new()
        {
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            if (databaseSettings.UseFakeMultitenancy)
            {
                throw new ArgumentException($"Cannot use {typeof(TTenantContext).Name} as TenantDbContext if UseFakeMultitenancy is enabled");
            }

            return services
                .AddBasePersistence<T, TTenant>(configuration)
                .AddTransient<IMultiTenantDatabaseInitializer, EFCoreMultiTenantDatabaseInitializer<T, TTenantContext, TTenant>>()
                ;
        }

        public static IServiceCollection AddPersistence<T, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext
            where TTenant : Tenant, new()
        {
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            if (databaseSettings.UseFakeMultitenancy)
            {
                return services
                    .AddBasePersistence<T, TTenant>(configuration)
                    .AddTransient<IMultiTenantDatabaseInitializer, InMemoryMultiTenantDatabaseInitializer<T, TTenant>>()
                    ;   
            }

            return services.AddPersistence<T, TenantDbContext<TTenant>, TTenant>(configuration);
        }

        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, IConfiguration configuration)
           where T : DbContext
        {
            return services.AddPersistence<T, Tenant>(configuration);
        }
    }
}
