using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Infrastructure.Multitenancy;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization
{

    public class MultiTenantDatabaseInitializer<TContext> : MultiTenantDatabaseInitializer<TContext, Tenant>
        where TContext : DbContext
    {

        public MultiTenantDatabaseInitializer(TenantDbContext<Tenant> tenantDbContext, IServiceProvider serviceProvider, ILogger<MultiTenantDatabaseInitializer<TContext>> logger)
            : base(tenantDbContext, serviceProvider, logger)
        {
        }
    }

    public class MultiTenantDatabaseInitializer<TContext, TTenantInfo> : IMultiTenantDatabaseInitializer
        where TContext : DbContext
        where TTenantInfo : Tenant, new()
    {
        private readonly TenantDbContext<TTenantInfo> tenantDbContext;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<MultiTenantDatabaseInitializer<TContext>> logger;

        public MultiTenantDatabaseInitializer(TenantDbContext<TTenantInfo> tenantDbContext, IServiceProvider serviceProvider, ILogger<MultiTenantDatabaseInitializer<TContext>> logger)
        {
            this.tenantDbContext = tenantDbContext;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
        {
            await InitializeTenantDbAsync(cancellationToken);

            foreach (var tenant in await tenantDbContext.TenantInfo.Where(x => x.IsActive).ToListAsync(cancellationToken))
            {
                await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
            }

            logger.LogInformation("For documentations and guides, visit https://www.fullstackhero.net");
            logger.LogInformation("To Sponsor this project, visit https://opencollective.com/fullstackhero");
        }


        public async Task InitializeApplicationDbForTenantAsync(Tenant tenant, CancellationToken cancellationToken)
        {
            await InitializeApplicationDbForTenantAsync(tenant as TTenantInfo, cancellationToken);
        }

        public async Task InitializeApplicationDbForTenantAsync(TTenantInfo tenant, CancellationToken cancellationToken)
        {
            // First create a new scope
            using var scope = serviceProvider.CreateScope();

            // Then set current tenant so the right connectionstring is used
            serviceProvider.GetRequiredService<IMultiTenantContextAccessor>()
                .MultiTenantContext = new MultiTenantContext<TTenantInfo>()
                {
                    TenantInfo = tenant
                };

            await scope.ServiceProvider
                .GetRequiredService<ApplicationDatabaseInitializer<TContext>>()
                .InitializeAsync(cancellationToken);
        }

        private async Task InitializeTenantDbAsync(CancellationToken cancellationToken)
        {
            if (tenantDbContext.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying Root Migrations.");
                await tenantDbContext.Database.MigrateAsync(cancellationToken);
            }

            await SeedRootTenantAsync(cancellationToken);
        }

        private async Task SeedRootTenantAsync(CancellationToken cancellationToken)
        {
            if (await tenantDbContext.TenantInfo.FirstOrDefaultAsync(x => x.Identifier == MultitenancyConstants.Root.Key, cancellationToken) is null)
            {
                var rootTenant = new TTenantInfo();

                rootTenant.Identifier = MultitenancyConstants.Root.Key;
                rootTenant.Name = MultitenancyConstants.Root.Name;
                rootTenant.ConnectionString = string.Empty;
                rootTenant.AdminEmail = MultitenancyConstants.Root.EmailAddress;
                rootTenant.IsActive = true;
                rootTenant.Issuer = null;

                // Add Default 1 Month Validity for all new tenants. Something like a DEMO period for tenants.
                rootTenant.ValidUpto = DateTime.UtcNow.AddMonths(1);

                rootTenant.SetValidity(DateTime.UtcNow.AddYears(1));

                tenantDbContext.TenantInfo.Add(rootTenant);

                await tenantDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

}