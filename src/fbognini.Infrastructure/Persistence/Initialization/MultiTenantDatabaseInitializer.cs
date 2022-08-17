using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Multitenancy;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization
{
    internal class MultiTenantDatabaseInitializer<TContext, TTenantContext, TTenantInfo> : IMultiTenantDatabaseInitializer
        where TContext : DbContext
        where TTenantContext: TenantDbContext<TTenantInfo>
        where TTenantInfo : Tenant, new()
    {
        private readonly TTenantContext tenantDbContext;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<MultiTenantDatabaseInitializer<TContext, TTenantContext, TTenantInfo>> logger;

        public MultiTenantDatabaseInitializer(TTenantContext tenantDbContext, IServiceProvider serviceProvider, ILogger<MultiTenantDatabaseInitializer<TContext, TTenantContext, TTenantInfo>> logger)
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

            logger.LogInformation("Multitenancy initialization completed");
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