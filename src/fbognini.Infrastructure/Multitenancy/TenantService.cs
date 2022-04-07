using AutoMapper;
using fbognini.Application.Multitenancy;
using fbognini.Application.Utilities;
using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;

namespace fbognini.Infrastructure.Multitenancy;

public class TenantService : ITenantService
{
    private readonly ISerializerService _serializer;

    private readonly IDistributedCache cache;

    private readonly DatabaseSettings _options;

    private readonly TenantManagementDbContext _context;

    private TenantDto _currentTenant;

    private readonly IMapper mapper;

    public TenantService(
        IOptions<DatabaseSettings> options,
        TenantManagementDbContext context,
        ISerializerService serializer,
        IMapper mapper, 
        IDistributedCache cache)
    {
        _options = options.Value;
        _context = context;
        _serializer = serializer;
        this.mapper = mapper;
        this.cache = cache;
    }

    public string GetConnectionString()
    {
        return _currentTenant?.ConnectionString;
    }

    public string GetDatabaseProvider()
    {
        return _options.DBProvider;
    }

    public TenantDto GetCurrentTenant()
    {
        return _currentTenant;
    }

    private void SetDefaultConnectionStringToCurrentTenant()
    {
        _currentTenant.ConnectionString = _options.ConnectionString;
    }

    public void SetCurrentTenant(string tenant)
    {
        if (_currentTenant != null)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }

        TenantDto tenantDto;
        string cacheKey = CacheKeys.GetCacheKey("tenant", tenant);
        byte[] cachedData = !string.IsNullOrWhiteSpace(cacheKey) ? cache.Get(cacheKey) : null;
        if (cachedData != null)
        {
            cache.Refresh(cacheKey);
            tenantDto = _serializer.Deserialize<TenantDto>(Encoding.Default.GetString(cachedData));
        }
        else
        {
            var tenantInfo = _context.Tenants.Where(a => a.Key == tenant).FirstOrDefault();
            tenantDto = mapper.Map<TenantDto>(tenantInfo);
            if (tenantDto != null)
            {
                var options = new DistributedCacheEntryOptions();
                byte[] serializedData = Encoding.Default.GetBytes(_serializer.Serialize(tenantDto));
                cache.Set(cacheKey, serializedData, options);
            }
        }

        if (tenantDto == null)
        {
            throw new InvalidTenantException("Invalid tenant");
        }

        if (tenantDto.Key != MultitenancyConstants.Root.Key)
        {
            if (!tenantDto.IsActive)
            {
                throw new InvalidTenantException("Tenant is inactive");
            }

            if (DateTime.UtcNow > tenantDto.ValidUpto)
            {
                throw new InvalidTenantException("Tenant validity is expired");
            }
        }

        _currentTenant = tenantDto;
        if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
        {
            SetDefaultConnectionStringToCurrentTenant();
        }
    }
}