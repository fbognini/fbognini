using fbognini.Infrastructure.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization
{
    public interface IMultiTenantDatabaseInitializer
    {
        Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    }

    public interface IMultiTenantDatabaseInitializer<TTenant>: IMultiTenantDatabaseInitializer
        where TTenant: Tenant
    {
        Task InitializeApplicationDbForTenantAsync(TTenant tenant, CancellationToken cancellationToken);
    }
}