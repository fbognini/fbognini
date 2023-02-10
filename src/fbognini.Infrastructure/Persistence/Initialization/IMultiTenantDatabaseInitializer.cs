using fbognini.Infrastructure.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization
{
    public interface IMultiTenantDatabaseInitializer
    {
        Task InitializeDatabasesAsync(CancellationToken cancellationToken);
        Task InitializeApplicationDbForTenantAsync(Tenant tenant, CancellationToken cancellationToken);
    }
}