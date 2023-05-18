using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization
{

    public class ApplicationSeederRunner<TContext>
        where TContext : DbContext
    {
        private readonly IServiceProvider serviceProvider;

        public ApplicationSeederRunner(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task RunSeedersAsync(TContext context, CancellationToken cancellationToken)
        {
            foreach (var seeder in serviceProvider.GetServices<ICustomSeeder<TContext>>())
            {
                await seeder.InitializeAsync(context, cancellationToken);
            }
        }
    }

}