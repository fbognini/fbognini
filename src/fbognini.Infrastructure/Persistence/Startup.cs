using fbognini.Infrastructure.Common;
using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Multitenancy;
using fbognini.Infrastructure.Outbox;
using fbognini.Infrastructure.Persistence.ConnectionString;
using fbognini.Infrastructure.Persistence.Initialization;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fbognini.Infrastructure.Persistence
{
    public static class Startup
    {
        private static IServiceCollection AddBasePersistence<T>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext, IBaseDbContext
        {
            LinqToDBForEFTools.Initialize();

            // TODO: We should probably add specific dbprovider/connectionstring setting for the tenantDb with a fallback to the main databasesettings
            var databaseSection = configuration.GetSection(nameof(DatabaseSettings));
            var databaseSettings = databaseSection.Get<DatabaseSettings>();

            var connectionString = databaseSettings.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DB ConnectionString is not configured.");
            }
            
            var dbProvider = databaseSettings.DBProvider;
            if (string.IsNullOrEmpty(dbProvider))
            {
                throw new InvalidOperationException("DB Provider is not configured.");
            }

            return services
                .Configure<DatabaseSettings>(databaseSection)
                .AddDbContextFactory<T>(GetContextOptionBuilder, lifetime: ServiceLifetime.Scoped) // Needed lifetime scoped to avoid issue "Cannot resolve scoped service ICurrentUserService from root provider."
                .AddTransient<ApplicationDatabaseInitializer<T>>()
                .AddImplementations(typeof(ICustomSeeder<T>), ServiceLifetime.Transient)
                .AddTransient<ApplicationSeederRunner<T>>()
                .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
                .AddTransient<IConnectionStringValidator, ConnectionStringValidator>();

            void GetContextOptionBuilder(DbContextOptionsBuilder contextOptions)
            {
                contextOptions.ConfigureDbProvider(databaseSettings.DBProvider!, connectionString);
                contextOptions.UseQueryTrackingBehavior(databaseSettings.TrackingBehavior);
            }
        }

        public static FinbuckleMultiTenantBuilder<TTenant> AddPersistenceAndMultitenancy<T, TTenantContext, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext, IBaseDbContext
            where TTenantContext : TenantDbContext<TTenant>
            where TTenant : Tenant, new()
        {
            services.AddDbContext<TTenantContext>(m => 
            {
                var connectionString = configuration[$"DatabaseSettings:{nameof(DatabaseSettings.ConnectionString)}"]!;
                var dbProvider = configuration[$"DatabaseSettings:{nameof(DatabaseSettings.DBProvider)}"]!;
                m.ConfigureDbProvider(dbProvider, connectionString);
            });


            return services
                .AddBasePersistence<T>(configuration)
                .AddOutboxProcessing<T, TTenant>()
                .AddMultiTenantInitializer<T, TTenantContext, TTenant>()
                .AddMultitenancy<TTenantContext, TTenant>(configuration);
        }


        public static FinbuckleMultiTenantBuilder<TTenant> AddPersistenceAndMultitenancy<T, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext, IBaseDbContext
            where TTenant : Tenant, new()
        {
            return services.AddBasePersistence<T>(configuration)
                .AddOutboxProcessing<T, TTenant>()
                .AddMultiTenantInitializer<T, TTenant>()
                .AddMultitenancy<TTenant>(configuration);
        }

        public static FinbuckleMultiTenantBuilder<Tenant> AddPersistenceAndMultitenancy<T>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext, IBaseDbContext
        {
            return services.AddPersistenceAndMultitenancy<T, Tenant>(configuration);
        }

        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext, IBaseDbContext
        {
            return services.AddBasePersistence<T>(configuration);
        }

        public static IServiceCollection AddMultiTenantInitializer<T, TTenantContext, TTenant>(this IServiceCollection services)
            where T : DbContext, IBaseDbContext
            where TTenantContext : TenantDbContext<TTenant>
            where TTenant : Tenant, new()
        {
            return services
                .AddTransient<IMultiTenantDatabaseInitializer, EFCoreMultiTenantDatabaseInitializer<T, TTenantContext, TTenant>>()
                .AddTransient<IMultiTenantDatabaseInitializer<TTenant>, EFCoreMultiTenantDatabaseInitializer<T, TTenantContext, TTenant>>();
        }

        public static IServiceCollection AddMultiTenantInitializer<T, TTenant>(this IServiceCollection services)
            where T : DbContext, IBaseDbContext
            where TTenant : Tenant, new()
        {
            return services
                .AddTransient<IMultiTenantDatabaseInitializer, EFCoreMultiTenantDatabaseInitializer<T, TTenant>>()
                .AddTransient<IMultiTenantDatabaseInitializer<TTenant>, EFCoreMultiTenantDatabaseInitializer<T, TTenant>>();
        }

        public static IServiceCollection AddMultiTenantInitializer<T>(this IServiceCollection services)
            where T : DbContext, IBaseDbContext
        {
            return services.AddMultiTenantInitializer<T, Tenant>();
        }
    }
}
