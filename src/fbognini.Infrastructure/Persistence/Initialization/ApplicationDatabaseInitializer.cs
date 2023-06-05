using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization
{

    public class ApplicationDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        private readonly TContext dbContext;
        private readonly ITenantInfo currentTenant;
        private readonly ApplicationSeederRunner<TContext> dbSeeder;
        private readonly ILogger<ApplicationDatabaseInitializer<TContext>> logger;

        public ApplicationDatabaseInitializer(TContext dbContext, ITenantInfo currentTenant, ApplicationSeederRunner<TContext> dbSeeder, ILogger<ApplicationDatabaseInitializer<TContext>> logger)
        {
            this.dbContext = dbContext;
            this.currentTenant = currentTenant;
            this.dbSeeder = dbSeeder;
            this.logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (dbContext.Database.GetMigrations().Any())
            {
                var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying migrations ({@Migrations}) for '{tenantId}' tenant.", pendingMigrations, currentTenant.Id);
                    await dbContext.Database.MigrateAsync(cancellationToken);
                }
            }

            if (await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                logger.LogInformation("Connection to {tenantId}'s Database Succeeded.", currentTenant.Id);

                await dbSeeder.RunSeedersAsync(dbContext, cancellationToken);
            }
        }
    }


}