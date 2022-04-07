using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace fbognini.Infrastructure.Multitenancy
{
    public static class MultitenancyExtensions
    {
        public static IServiceCollection AddMultitenancy<T, TA>(this IServiceCollection services, IConfiguration config)
        where T : TenantManagementDbContext
        where TA : DbContext
        {
            var section = config.GetSection(nameof(DatabaseSettings));
            var settings = new DatabaseSettings();
            section.Bind(settings);
            services.Configure<DatabaseSettings>(section);

            string rootConnectionString = settings.ConnectionString;
            string dbProvider = settings.DBProvider;
            if (string.IsNullOrEmpty(dbProvider)) throw new Exception("DB Provider is not configured.");

            services.AddDbContext<T>(m => m.UseSqlServer(rootConnectionString));

            services.SetupDatabases<T, TA>(settings);
            return services;
        }

        private static IServiceCollection SetupDatabases<T, TA>(this IServiceCollection services, DatabaseSettings options)
        where T : TenantManagementDbContext
        where TA : DbContext
        {
            var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger(typeof(MultitenancyExtensions));

            logger.LogDebug("Setup database to {@options}", options);

            services.AddDbContext<TA>(m => m.UseSqlServer(options.ConnectionString));

            if (dbContext.Database.GetMigrations().Any())
            {
                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    logger.LogInformation("There are pending migrations, let's migrate it");
                    dbContext.Database.Migrate();
                }

                if (dbContext.Database.CanConnect())
                {
                    try
                    {
                        SeedRootTenant(dbContext, options, logger);
                        foreach (var tenant in dbContext.Tenants.ToList())
                        {
                            services.SetupTenantDatabase<TA>(options, tenant);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Cannot setup tenants' database");
                    }
                }
            }

            return services;
        }

        private static void SeedRootTenant<T>(T dbContext, DatabaseSettings options, ILogger logger)
            where T : TenantManagementDbContext
        {

            if (!dbContext.Tenants.Any(t => t.Key == MultitenancyConstants.Root.Key))
            {
                logger.LogInformation("Root tenant doesn't exist, let's add it to Tenant table");
                var rootTenant = new Tenant(MultitenancyConstants.Root.Name, MultitenancyConstants.Root.Key, MultitenancyConstants.Root.EmailAddress, options.ConnectionString);
                rootTenant.SetValidity(DateTime.UtcNow.AddYears(1));
                dbContext.Tenants.Add(rootTenant);
                dbContext.SaveChanges();
            }
        }

        private static IServiceCollection SetupTenantDatabase<TA>(this IServiceCollection services, DatabaseSettings options, Tenant tenant)
        where TA : DbContext
        {
            var scope = services.BuildServiceProvider().CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger>();

            logger.LogDebug("Setup tenant {tenant} database to {@options}", tenant.Name, options);

            string tenantConnectionString = string.IsNullOrEmpty(tenant.ConnectionString) ? options.ConnectionString : tenant.ConnectionString;
            services.AddDbContext<TA>(m => m.UseSqlServer(tenantConnectionString));
            return services;
        }
    }
}
