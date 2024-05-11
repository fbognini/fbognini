using fbognini.Application.Multitenancy;
using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Persistence;
using Finbuckle.MultiTenant;
using LinqToDB;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox
{
    public interface IOutboxMessageProcessor
    {
        Task<int> ProcessDomainEvents(CancellationToken cancellationToken);
        Task<int> ProcessDomainEvents(bool ignoreApplicationFiter, CancellationToken cancellationToken);
    }

    internal class OutboxMessageProcessor<TTenant>: IOutboxMessageProcessor
        where TTenant : Tenant, new()
    {
        private readonly ILogger<OutboxMessageProcessor<TTenant>> logger;

        private readonly IServiceProvider serviceProvider;
        private readonly IBaseDbContext baseDbContext;
        private readonly ITenantService<TTenant> tenantService;
        private readonly OutboxSettings outboxSettings;
        private readonly DatabaseSettings databaseSettings;

        public OutboxMessageProcessor(
            // I'll use it in a different scope. Throw exception if not in DI.
            IOutboxMessagePublisher _, 
            ILogger<OutboxMessageProcessor<TTenant>> logger,
            IServiceProvider serviceProvider,
            IBaseDbContext baseDbContext,
            ITenantService<TTenant> tenantService,
            IOptions<OutboxSettings> outboxSettings,
            IOptions<DatabaseSettings> databaseSettings)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.baseDbContext = baseDbContext;
            this.tenantService = tenantService;
            this.outboxSettings = outboxSettings.Value;
            this.databaseSettings = databaseSettings.Value;
        }

        public Task<int> ProcessDomainEvents(CancellationToken cancellationToken) => ProcessDomainEvents(false, cancellationToken);

        public async Task<int> ProcessDomainEvents(bool ignoreApplicationFiter, CancellationToken cancellationToken)
        {
            int noOfOutboxMessages;
            do
            {
                using var transaction = ((DbContext)baseDbContext).Database.BeginTransaction();

                var outboxMessages = GetOutboxMessages(ignoreApplicationFiter);
                noOfOutboxMessages = outboxMessages.Count;

                logger.LogInformation("{NoOfOutboxMessages} outbox message(s) found", noOfOutboxMessages);

                foreach (var outboxMessage in outboxMessages)
                {
                    var propertys = new Dictionary<string, object?>()
                    {
                        ["OutboxMessageId"] = outboxMessage.Id,
                        ["Tenant"] = outboxMessage.Tenant,
                    };

                    using (logger.BeginScope(propertys))
                    {
                        await Process(outboxMessage, cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
            }
            while (noOfOutboxMessages > 0);

            return noOfOutboxMessages;
        }


        private async Task Process(OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            var outboxMessageId = outboxMessage.Id;
            var outboxTenant = outboxMessage.Tenant;

            try
            {
                var tenant = await GetTenant(outboxTenant);
                if (tenant is null && !string.IsNullOrWhiteSpace(outboxTenant))
                {
                    logger.LogWarning("Tenat {Tenant} for outbox message {OutboxMessageId} was not found", outboxTenant, outboxMessageId);
                }

                serviceProvider.GetRequiredService<IMultiTenantContextAccessor>().MultiTenantContext = new MultiTenantContext<TTenant>()
                {
                    TenantInfo = tenant,
                    StrategyInfo = null,
                    StoreInfo = null
                };

                using (var outboxScope = serviceProvider.CreateScope())
                {
                    var publisher = outboxScope.ServiceProvider.GetRequiredService<IOutboxMessagePublisher>();

                    logger.LogDebug("Start processing outbox message {OutboxMessageId} as {OutboxMessageType}", outboxMessageId, outboxMessage.Type);

                    await publisher.Publish(outboxMessage, cancellationToken);
                }

                outboxMessage.SetAsProcessed(DateTime.UtcNow);

                logger.LogInformation("Outbox message {OutboxMessageId} has been processed", outboxMessageId);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Exception while processing outbox message {OutboxMessageId}",
                    outboxMessage.Id);

                outboxMessage.SetAsProcessedWithError(DateTime.UtcNow, exception.Message);
            }

            await baseDbContext.BaseSaveChangesAsync(cancellationToken);
        }

        private async Task<TTenant?> GetTenant(string outboxTenant)
        {
            if (string.IsNullOrWhiteSpace(outboxTenant))
            {
                return null;
            }

            return await tenantService.GetByIdAsync(outboxTenant);
        }

        private List<OutboxMessage> GetOutboxMessages(bool ignoreApplicationFiter)
        {
            var applicationName = Assembly.GetEntryAssembly()!.GetName().Name!;

            var linq2dbTable = baseDbContext.OutboxMessages
                .ToLinqToDBTable();

            if (databaseSettings.DBProvider == "mssql")
            {
                linq2dbTable = linq2dbTable
                        .TableHint(SqlServerHints.Table.ReadPast)
                        .TableHint(SqlServerHints.Table.UpdLock);
            }

            var query = linq2dbTable
                // GlobalQueryFilter for tenant
                .IgnoreFilters()
                .Where(x => !x.ProcessedOnUtc.HasValue);

            if (ignoreApplicationFiter)
            {
                if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.Me)
                {
                    query = query.Where(x => x.Application == applicationName);
                }
                else if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.Custom &&
                    outboxSettings.Applications is not null)
                {
                    var applications = outboxSettings.Applications.Split(new[] { ',', ';' }).ToList();
                    query = query.Where(x => applications.Contains(x.Application));
                }
            }

            var events = query
                .OrderBy(o => o.OccurredOnUtc)
                .Take(outboxSettings.BatchSize)
                .ToList();

            return events;
        }

    }
}
