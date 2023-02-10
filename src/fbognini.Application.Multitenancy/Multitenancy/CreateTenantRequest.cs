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
}