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

namespace fbognini.Infrastructure.Multitenancy;

public class TenantService : ITenantService
{
    private readonly IMapper mapper;
    private readonly IMultiTenantStore<Tenant> _tenantStore;
    private readonly IConnectionStringSecurer _csSecurer;
    private readonly IMultiTenantDatabaseInitializer _dbInitializer;
    //private readonly IStringLocalizer _t;
    private readonly DatabaseSettings _dbSettings;

    public TenantService(
        IMultiTenantStore<Tenant> tenantStore,
        IConnectionStringSecurer csSecurer,
        IMultiTenantDatabaseInitializer dbInitializer,
        //IStringLocalizer<TenantService> localizer,
        IOptions<DatabaseSettings> dbSettings, IMapper mapper)
    {
        _tenantStore = tenantStore;
        _csSecurer = csSecurer;
        _dbInitializer = dbInitializer;
        //_t = localizer;
        _dbSettings = dbSettings.Value;
        this.mapper = mapper;
    }

    public async Task<List<TenantDto>> GetAllAsync()
    {
        var tenants = mapper.Map<List<TenantDto>>(await _tenantStore.GetAllAsync());
        tenants.ForEach(t => t.ConnectionString = _csSecurer.MakeSecure(t.ConnectionString));
        return tenants;
    }

    public async Task<bool> ExistsWithIdAsync(string id) =>
        await _tenantStore.TryGetAsync(id) is not null;

    public async Task<bool> ExistsWithNameAsync(string name) =>
        (await _tenantStore.GetAllAsync()).Any(t => t.Name == name);

    public async Task<TenantDto> GetByIdAsync(string id) =>
        mapper.Map<TenantDto>(await GetTenantInfoAsync(id));

    public async Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        if (request.ConnectionString?.Trim() == _dbSettings.ConnectionString?.Trim()) request.ConnectionString = string.Empty;

        var tenant = new Tenant(request.Id, request.Name, request.ConnectionString, request.AdminEmail, request.Issuer);
        await _tenantStore.TryAddAsync(tenant);

        // TODO: run this in a hangfire job? will then have to send mail when it's ready or not
        try
        {
            await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
        }
        catch
        {
            await _tenantStore.TryRemoveAsync(request.Id);
            throw;
        }

        return tenant.Id;
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

        await _tenantStore.TryUpdateAsync(tenant);

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

        await _tenantStore.TryUpdateAsync(tenant);

        //return _t[$"Tenant {0} is now Deactivated.", id];
        return String.Format("Tenant {0} is now Deactivated.", id);
    }

    public async Task<string> UpdateSubscription(string id, DateTime extendedExpiryDate)
    {
        var tenant = await GetTenantInfoAsync(id);

        tenant.SetValidity(extendedExpiryDate);

        await _tenantStore.TryUpdateAsync(tenant);

        //return _t[$"Tenant {0}'s Subscription Upgraded. Now Valid till {1}.", id, tenant.ValidUpto];
        return String.Format("Tenant {0}'s Subscription Upgraded. Now Valid till {1}.", id, tenant.ValidUpto);
    }

    private async Task<Tenant> GetTenantInfoAsync(string id) =>
        await _tenantStore.TryGetAsync(id)
            //?? throw new NotFoundException(_t["{0} {1} Not Found.", typeof(Tenant).Name, id]);
            ?? throw new NotFoundException(String.Format("{0} {1} Not Found.", typeof(Tenant).Name, id));
}