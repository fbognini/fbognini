using fbognini.Application.Entities;
using fbognini.Core.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{
    public class TenantDto: Mappable<TenantDto, Tenant>
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ConnectionString { get; set; }
        public string AdminEmail { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime ValidUpto { get; set; }
        public string Issuer { get; set; }
    }
}
