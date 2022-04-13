using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization;

public class ApplicationSeederRunner<TContext>
    where TContext : DbContext
{
    private readonly ICustomSeeder<TContext>[] _seeders;

    public ApplicationSeederRunner(IServiceProvider serviceProvider) =>
        _seeders = serviceProvider.GetServices<ICustomSeeder<TContext>>().ToArray();

    public async Task RunSeedersAsync(TContext context, CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.InitializeAsync(context, cancellationToken);
        }
    }
}