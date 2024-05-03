using fbognini.Application.Multitenancy;
using fbognini.Core.Domain;
using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Persistence;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public interface IOutboxMessagesListener
{
    Task PublishDomainMemoryEventAsync(IReadOnlyList<IDomainMemoryEvent> domainMemoryEvents, CancellationToken cancellationToken);

    void Notify();
}

internal class OutboxMessagesListenerService<TTenant> : BackgroundService, IOutboxMessagesListener
    where TTenant : Tenant, new()
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxMessagesListenerService<TTenant>> logger;

    private CancellationTokenSource _wakeupCancelationTokenSource = new();
    private readonly string _applicationName = Assembly.GetEntryAssembly()!.GetName().Name!;

    public OutboxMessagesListenerService(IServiceScopeFactory scopeFactory, ILogger<OutboxMessagesListenerService<TTenant>> logger)
    {
        _scopeFactory = scopeFactory;
        this.logger = logger;
    }

    public async Task PublishDomainMemoryEventAsync(IReadOnlyList<IDomainMemoryEvent> domainMemoryEvents, CancellationToken cancellationToken)
    {
        if (domainMemoryEvents is null || domainMemoryEvents.Count == 0)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetService<IOutboxMessageProcessor>();
        if (processor is null)
        {
            logger.LogWarning("No IOutboxMessageProcessor has been registered, memory events can't be processed");
            return;
        }

        foreach (var domainMemoryEvent in domainMemoryEvents)
        {
            await processor.Process(domainMemoryEvent, cancellationToken);
        }
    }

    public void Notify()
    {
        logger.LogDebug("Listener has been notified");

        _wakeupCancelationTokenSource.Cancel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var outboxSettings = scope.ServiceProvider.GetRequiredService<IOptions<OutboxSettings>>().Value;
            if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.None)
            {
                logger.LogInformation("Outbox messages won't be processed by {ApplicationName}", _applicationName);
                return;
            }

            var processor = scope.ServiceProvider.GetService<IOutboxMessageProcessor>();
            if (processor is null)
            {
                logger.LogWarning("No IOutboxMessageProcessor has been registered, outbox messages won't be processed by {ApplicationName}", _applicationName);
                return;
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishDomainEvents(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during domain events publishing");

                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task PublishDomainEvents(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int interval = -1;

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IBaseDbContext>();
                var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService<TTenant>>();
                var outboxSettings = scope.ServiceProvider.GetRequiredService<IOptions<OutboxSettings>>().Value;
                if (outboxSettings.IntervalInSeconds > 0)
                {
                    interval = outboxSettings.IntervalInSeconds * 1000;
                }

                var outboxMessages = GetOutboxMessages(dbContext, outboxSettings);

                logger.LogInformation("{NoOfOutboxMessages} outbox message(s) found", outboxMessages.Count);

                foreach (var outboxMessage in outboxMessages)
                {
                    var outboxMessageId = outboxMessage.Id;
                    var outboxTenant = outboxMessage.Tenant;

                    var propertys = new Dictionary<string, object?>()
                    {
                        ["OutboxMessageId"] = outboxMessageId,
                        ["Tenant"] = outboxTenant,
                    };

                    using (logger.BeginScope(propertys))
                    {

                        try
                        {
                            var tenant = await GetTenant(tenantService, outboxMessageId, outboxTenant);

                            scope.ServiceProvider.GetRequiredService<IMultiTenantContextAccessor>().MultiTenantContext = new MultiTenantContext<TTenant>()
                            {
                                TenantInfo = tenant,
                                StrategyInfo = null,
                                StoreInfo = null
                            };

                            using (var outboxScope = _scopeFactory.CreateScope())
                            {
                                var processor = outboxScope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();

                                logger.LogDebug("Start processing outbox message {OutboxMessageId} as {OutboxMessageType}", outboxMessage.Id, outboxMessage.Type);

                                await processor.Process(outboxMessage, stoppingToken);
                            }

                            outboxMessage.SetAsProcessed(DateTime.UtcNow);

                            logger.LogInformation("Outbox message {OutboxMessageId} has been processed", outboxMessage.Id);
                        }
                        catch (Exception exception)
                        {
                            logger.LogError(
                                exception,
                                "Exception while processing outbox message {OutboxMessageId}",
                                outboxMessage.Id);

                            outboxMessage.SetAsProcessedWithError(DateTime.UtcNow, exception.Message);
                        }

                        await dbContext.BaseSaveChangesAsync(stoppingToken);
                    }
                }
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_wakeupCancelationTokenSource.Token, stoppingToken);
            try
            {
                if (interval == -1)
                {
                    logger.LogInformation("Listening for a new event");
                }
                else
                {
                    logger.LogInformation("Listening for a new event of waiting for {Internval}ms", interval);
                }

                await Task.Delay(interval, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                if (_wakeupCancelationTokenSource.Token.IsCancellationRequested)
                {
                    logger.LogInformation("Outbox listener is awake");

                    var tmp = _wakeupCancelationTokenSource;
                    _wakeupCancelationTokenSource = new CancellationTokenSource();
                    tmp.Dispose();
                }
                else if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Outbox listener is shutting down");
                }
            }
        }
    }

    private async Task<TTenant?> GetTenant(ITenantService<TTenant> tenantService, Guid outboxMessageId, string outboxTenant)
    {
        if (string.IsNullOrWhiteSpace(outboxTenant))
        {
            return null;
        }

        var tenant = await tenantService.GetByIdAsync(outboxTenant);
        if (tenant is null)
        {
            logger.LogWarning("Tenat {Tenant} for outbox message {OutboxMessageId} was not found", outboxTenant, outboxMessageId);
        }

        return tenant;
    }

    private List<OutboxMessage> GetOutboxMessages(IBaseDbContext dbContext, OutboxSettings outboxSettings)
    {
        var query = dbContext.OutboxMessages.Where(x => !x.ProcessedOnUtc.HasValue);

        if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.Me)
        {
            query = query.Where(x => x.Application == _applicationName);
        }
        else if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.Custom &&
            outboxSettings.Applications is not null)
        {
            var applications = outboxSettings.Applications.Split(new[] { ',', ';' }).ToList();
            query = query.Where(x => applications.Contains(x.Application));
        }

        var events = query
            .OrderBy(o => o.OccurredOnUtc)
            .Take(outboxSettings.BatchSize)
            .ToList();

        return events;
    }
}