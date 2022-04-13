using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence.Initialization;

public class ApplicationDatabaseInitializer<TContext>
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly ITenantInfo _currentTenant;
    private readonly ApplicationSeederRunner<TContext> _dbSeeder;
    private readonly ILogger<ApplicationDatabaseInitializer<TContext>> _logger;

    public ApplicationDatabaseInitializer(TContext dbContext, ITenantInfo currentTenant, ApplicationSeederRunner<TContext> dbSeeder, ILogger<ApplicationDatabaseInitializer<TContext>> logger)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
        _dbSeeder = dbSeeder;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.GetMigrations().Any())
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                _logger.LogInformation("Applying Migrations for '{tenantId}' tenant.", _currentTenant.Id);
                await _dbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await _dbContext.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogInformation("Connection to {tenantId}'s Database Succeeded.", _currentTenant.Id);

                await _dbSeeder.RunSeedersAsync(_dbContext, cancellationToken);
            }
        }
    }
}
