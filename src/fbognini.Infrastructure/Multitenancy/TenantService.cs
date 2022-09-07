using AutoMapper;
using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Application.Persistence;
using fbognini.Core.Exceptions;
using Finbuckle.MultiTenant;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using fbognini.Infrastructure.Persistence.Initialization;

namespace fbognini.Infrastructure.Multitenancy
{

    public class TenantService<TTenant> : ITenantService
            where TTenant : Tenant, new()
    {
        private readonly IMapper mapper;
        private readonly IMultiTenantStore<TTenant> tenantStore;
        private readonly IConnectionStringSecurer csSecurer;
        private readonly IMultiTenantDatabaseInitializer _dbInitializer;
        private readonly DatabaseSettings _dbSettings;

        public TenantService(
            IMultiTenantStore<TTenant> tenantStore,
            IConnectionStringSecurer csSecurer,
            IMultiTenantDatabaseInitializer dbInitializer,
            IOptions<DatabaseSettings> dbSettings, IMapper mapper)
        {
            this.tenantStore = tenantStore;
            this.csSecurer = csSecurer;
            _dbInitializer = dbInitializer;
            _dbSettings = dbSettings.Value;
            this.mapper = mapper;
        }

        public async Task<List<TenantDto>> GetAllAsync()
        {
            var tenants = mapper.Map<List<TenantDto>>(await tenantStore.GetAllAsync());
            tenants.ForEach(t => t.ConnectionString = csSecurer.MakeSecure(t.ConnectionString));
            return tenants;
        }

        public async Task<bool> ExistsWithIdAsync(string id) =>
            !(await tenantStore.TryGetAsync(id) is null);

        public async Task<bool> ExistsWithNameAsync(string name) =>
            (await tenantStore.GetAllAsync()).Any(t => t.Name == name);

        public async Task<TenantDto> GetByIdAsync(string id)
        {
            var tenant = mapper.Map<TenantDto>(await GetTenantInfoAsync(id));
            tenant.ConnectionString = csSecurer.MakeSecure(tenant.ConnectionString);

            return tenant;
        }

        public async Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken)
        {
            if (request.ConnectionString?.Trim() == _dbSettings.ConnectionString?.Trim()) request.ConnectionString = string.Empty;

            var tenant = new TTenant
            {
                Identifier = request.Identifier,
                Name = request.Name,
                ConnectionString = request.ConnectionString ?? string.Empty,
                AdminEmail = request.AdminEmail,
                IsActive = true,
                Issuer = request.Issuer,
                // Add Default 1 Month Validity for all new tenants. Something like a DEMO period for tenants.
                ValidUpto = DateTime.UtcNow.AddMonths(1)
            };

            await tenantStore.TryAddAsync(tenant);

            // TODO: run this in a hangfire job? will then have to send mail when it's ready or not
            try
            {
                await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
            }
            catch
            {
                await tenantStore.TryRemoveAsync(request.Identifier);
                throw;
            }

            return tenant.Id.ToString();
        }

        public async Task<string> ActivateAsync(string id)
        {
            var tenant = await GetTenantInfoAsync(id);

            if (tenant.IsActive)
            {
                //throw new ConflictException(_t["Tenant is already Activated."]);
                throw new ConflictException("Tenant is already Activated.");
            }

            tenant.Activate();

            await tenantStore.TryUpdateAsync(tenant);

            //return _t["Tenant {0} is now Activated.", id];
            return String.Format("Tenant {0} is now Activated.", id);
        }

        public async Task<string> DeactivateAsync(string id)
        {
            var tenant = await GetTenantInfoAsync(id);

            if (!tenant.IsActive)
            {
                //throw new ConflictException(_t["Tenant is already Deactivated."]);
                throw new ConflictException("Tenant is already Deactivated.");
            }

            tenant.Deactivate();

            await tenantStore.TryUpdateAsync(tenant);

            //return _t[$"Tenant {0} is now Deactivated.", id];
            return String.Format("Tenant {0} is now Deactivated.", id);
        }

        public async Task<string> UpdateSubscription(string id, DateTime extendedExpiryDate)
        {
            var tenant = await GetTenantInfoAsync(id);

            tenant.SetValidity(extendedExpiryDate);

            await tenantStore.TryUpdateAsync(tenant);

            //return _t[$"Tenant {0}'s Subscription Upgraded. Now Valid till {1}.", id, tenant.ValidUpto];
            return String.Format("Tenant {0}'s Subscription Upgraded. Now Valid till {1}.", id, tenant.ValidUpto);
        }

        private async Task<TTenant> GetTenantInfoAsync(string id) =>
            await tenantStore.TryGetAsync(id)
                //?? throw new NotFoundException(_t["{0} {1} Not Found.", typeof(Tenant).Name, id]);
                ?? throw new NotFoundException(String.Format("{0} {1} Not Found.", typeof(Tenant).Name, id));
    }

}