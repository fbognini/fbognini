
//using fbognini.Core.Data;
//using System.Threading.Tasks;

//namespace fbognini.Application.Multitenancy;

//public interface ITenantManager
//{
//    public Task<Result<TenantDto>> GetByKeyAsync(string key);

//    public Task<Result<List<TenantDto>>> GetAllAsync();

//    public Task<Result<object>> CreateTenantAsync(CreateTenantRequest request);

//    Task<Result<object>> UpgradeSubscriptionAsync(UpgradeSubscriptionRequest request);

//    Task<Result<object>> DeactivateTenantAsync(string tenant);

//    Task<Result<object>> ActivateTenantAsync(string tenant);
//}