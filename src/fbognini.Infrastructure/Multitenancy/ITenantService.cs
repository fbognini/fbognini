using fbognini.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{
    public class CreateTenantRequest
    {
        public string Identifier { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ConnectionString { get; set; }
        public string AdminEmail { get; set; } = default!;
        public string Issuer { get; set; }
    }

    public interface ITenantService<TTenant>
        where TTenant : Tenant, new()
    {
        Task<List<TTenant>> GetAllAsync();
        Task<bool> ExistsWithIdAsync(string id);
        Task<bool> ExistsWithNameAsync(string name);
        Task<TTenant> GetByIdAsync(string id);
        Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken);
        Task<string> ActivateAsync(string id);
        Task<string> DeactivateAsync(string id);
        Task<string> UpdateSubscription(string id, DateTime extendedExpiryDate);
    }
}