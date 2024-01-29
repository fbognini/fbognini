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
        private static IServiceCollection AddBasePersistence<T>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext
        {
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
                .AddDbContextFactory<T>(GetContextOptionBuilder, lifetime: ServiceLifetime.Scoped)
                .AddTransient<ApplicationDatabaseInitializer<T>>()
                .AddImplementations(typeof(ICustomSeeder<T>), ServiceLifetime.Transient)
                .AddTransient<ApplicationSeederRunner<T>>()
                .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
                .AddTransient<IConnectionStringValidator, ConnectionStringValidator>();

            void GetContextOptionBuilder(DbContextOptionsBuilder contextOptions)
            {
                contextOptions.UseSqlServer(connectionString, providerOptions =>
                {

                });
                contextOptions.UseQueryTrackingBehavior(databaseSettings.TrackingBehavior);
            }
        }

        public static IServiceCollection AddPersistence<T, TTenantContext, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext
            where TTenantContext : TenantDbContext<TTenant>
            where TTenant : Tenant, new()
        {
            services.AddDbContext<TTenantContext>(m => 
            {
                m.UseSqlServer(configuration["DatabaseSettings:ConnectionString"]);
            });


            return services
                .AddBasePersistence<T>(configuration)
                .AddTransient<IMultiTenantDatabaseInitializer, EFCoreMultiTenantDatabaseInitializer<T, TTenantContext, TTenant>>()
                .AddTransient<IMultiTenantDatabaseInitializer<TTenant>, EFCoreMultiTenantDatabaseInitializer<T, TTenantContext, TTenant>>();
        }

        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, IConfiguration configuration)
           where T : DbContext
        {
            return services.AddBasePersistence<T>(configuration);
        }
    }
}
