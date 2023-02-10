using fbognini.Infrastructure.Entities;
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
    internal abstract class BaseMultiTenantDatabaseInitializer<TContext, TTenantInfo>
        where TContext : DbContext
        where TTenantInfo : Tenant, new()
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IMultiTenantStore<TTenantInfo> store;

        public BaseMultiTenantDatabaseInitializer(IServiceProvider serviceProvider, IMultiTenantStore<TTenantInfo> store)
        {
            this.serviceProvider = serviceProvider;
            this.store = store;
        }

        public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
        {
            var tenants = await store.GetAllAsync();
            foreach (var tenant in tenants.Where(x => x.IsActive))
            {
                await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
            }
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
    }

    internal class InMemoryMultiTenantDatabaseInitializer<TContext, TTenantInfo>
        : BaseMultiTenantDatabaseInitializer<TContext, TTenantInfo>, IMultiTenantDatabaseInitializer
        where TContext : DbContext
        where TTenantInfo : Tenant, new()
    {
        private readonly ILogger<InMemoryMultiTenantDatabaseInitializer<TContext, TTenantInfo>> logger;

        public InMemoryMultiTenantDatabaseInitializer(
            IServiceProvider serviceProvider,
            IMultiTenantStore<TTenantInfo> store,
            ILogger<InMemoryMultiTenantDatabaseInitializer<TContext, TTenantInfo>> logger)
            : base(serviceProvider, store)
        {
            this.logger = logger;
        }

        public async new Task InitializeDatabasesAsync(CancellationToken cancellationToken)
        {
            await base.InitializeDatabasesAsync(cancellationToken);
            logger.LogInformation("Multitenancy (in memory) initialization completed");
        }
    }

    internal class EFCoreMultiTenantDatabaseInitializer<TContext, TTenantContext, TTenantInfo> 
        : BaseMultiTenantDatabaseInitializer<TContext, TTenantInfo>, IMultiTenantDatabaseInitializer
        where TContext : DbContext
        where TTenantContext: TenantDbContext<TTenantInfo>
        where TTenantInfo : Tenant, new()
    {
        private readonly TTenantContext tenantDbContext;
        private readonly ILogger<EFCoreMultiTenantDatabaseInitializer<TContext, TTenantContext, TTenantInfo>> logger;

        public EFCoreMultiTenantDatabaseInitializer(
            IServiceProvider serviceProvider,
            IMultiTenantStore<TTenantInfo> store,
            TTenantContext tenantDbContext,
            ILogger<EFCoreMultiTenantDatabaseInitializer<TContext, TTenantContext, TTenantInfo>> logger)
            : base(serviceProvider, store)
        {
            this.tenantDbContext = tenantDbContext;
            this.logger = logger;
        }

        public async new Task InitializeDatabasesAsync(CancellationToken cancellationToken)
        {
            await InitializeTenantDbAsync(cancellationToken);
            await base.InitializeDatabasesAsync(cancellationToken);

            logger.LogInformation("Multitenancy (ef core) initialization completed");
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
                var rootTenant = new TTenantInfo
                {
                    Identifier = MultitenancyConstants.Root.Key,
                    Name = MultitenancyConstants.Root.Name,
                    ConnectionString = string.Empty,
                    AdminEmail = MultitenancyConstants.Root.EmailAddress,
                    IsActive = true,
                    Issuer = null,
                    ValidUpto = DateTime.UtcNow.AddYears(1)
                };

                tenantDbContext.TenantInfo.Add(rootTenant);

                await tenantDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

}