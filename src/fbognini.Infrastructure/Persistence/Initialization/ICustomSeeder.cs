using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization
{

    public interface ICustomSeeder<TContext>
        where TContext : DbContext
    {
        Task InitializeAsync(TContext context, CancellationToken cancellationToken);
    }

}